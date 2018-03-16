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
using SafeExamBrowser.Contracts.Communication;
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

			var configuration = sut.GetConfiguration();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded)), Times.Once);

			Assert.IsInstanceOfType(configuration, typeof(ClientConfiguration));
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustFailIfConfigurationNotRetrieved()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded))).Returns<Response>(null);

			sut.GetConfiguration();
		}

		[TestMethod]
		public void MustCorrectlyInformClientReady()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			sut.InformClientReady();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady)), Times.Once);
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustFailIfClientReadyNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns<Response>(null);

			sut.InformClientReady();
		}

		[TestMethod]
		public void MustCorrectlyRequestReconfiguration()
		{
			var url = "sebs://some/url.seb";
			var response = new ReconfigurationResponse
			{
				Accepted = true
			};

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationUrl == url))).Returns(response);

			var accepted = sut.RequestReconfiguration(url);

			proxy.Verify(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationUrl == url)), Times.Once);

			Assert.IsTrue(accepted);
		}

		[TestMethod]
		public void MustCorrectlyHandleDeniedReconfigurationRequest()
		{
			var url = "sebs://some/url.seb";
			var response = new ReconfigurationResponse
			{
				Accepted = false
			};

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationUrl == url))).Returns(response);

			var accepted = sut.RequestReconfiguration(url);

			Assert.IsFalse(accepted);
		}

		[TestMethod]
		public void MustNotFailIfIncorrectResponseToReconfigurationRequest()
		{
			var url = "sebs://some/url.seb";

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationUrl == url))).Returns<Response>(null);

			var accepted = sut.RequestReconfiguration(url);

			Assert.IsFalse(accepted);
		}

		[TestMethod]
		public void MustCorrectlyRequestShutdown()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			sut.RequestShutdown();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown)), Times.Once);
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustFailIfShutdownRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns<Response>(null);

			sut.RequestShutdown();
		}
	}
}
