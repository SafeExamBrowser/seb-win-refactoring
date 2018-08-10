/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Proxies;

namespace SafeExamBrowser.Core.UnitTests.Communication.Proxies
{
	[TestClass]
	public class RuntimeProxyTests
	{
		private Mock<ILogger> logger;
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<IProxyObject> proxy;
		private RuntimeProxy sut;

		[TestInitialize]
		public void Initialize()
		{
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			logger = new Mock<ILogger>();
			proxyObjectFactory = new Mock<IProxyObjectFactory>();
			proxy = new Mock<IProxyObject>();

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			sut = new RuntimeProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object);
			sut.Connect(Guid.NewGuid());
		}

		[TestMethod]
		public void MustCorrectlyRetrieveConfiguration()
		{
			var response = new ConfigurationResponse
			{
				Configuration = new ClientConfiguration()
			};

			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded))).Returns(response);

			var communication = sut.GetConfiguration();
			var configuration = communication.Value.Configuration;

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded)), Times.Once);

			Assert.IsTrue(communication.Success);
			Assert.IsInstanceOfType(configuration, typeof(ClientConfiguration));
		}

		[TestMethod]
		public void MustFailIfConfigurationNotRetrieved()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded))).Returns<Response>(null);

			var communication = sut.GetConfiguration();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyInformClientReady()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InformClientReady();

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfClientReadyNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns<Response>(null);

			var communication = sut.InformClientReady();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestReconfiguration()
		{
			var url = "file:///C:/Some/file/url.seb";

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == url))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestReconfiguration(url);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == url)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfReconfigurationRequestNotAcknowledged()
		{
			var url = "file:///C:/Some/file/url.seb";

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == url))).Returns<Response>(null);

			var communication = sut.RequestReconfiguration(url);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestShutdown()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestShutdown();

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfShutdownRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns<Response>(null);

			var communication = sut.RequestShutdown();

			Assert.IsFalse(communication.Success);
		}
	}
}
