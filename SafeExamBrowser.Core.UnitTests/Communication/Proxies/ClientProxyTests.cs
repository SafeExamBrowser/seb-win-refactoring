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
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Proxies;

namespace SafeExamBrowser.Core.UnitTests.Communication.Proxies
{
	[TestClass]
	public class ClientProxyTests
	{
		private Mock<ILogger> logger;
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<IProxyObject> proxy;
		private ClientProxy sut;

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

			sut = new ClientProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object);
			sut.Connect(Guid.NewGuid());
		}

		[TestMethod]
		public void MustCorrectlyInitiateShutdown()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InitiateShutdown();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown)), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustIndicateIfShutdownCommandNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown))).Returns<Response>(null);

			var communication = sut.InitiateShutdown();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestAuthentication()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate))).Returns(new AuthenticationResponse());

			var communication = sut.RequestAuthentication();
			var response = communication.Value;

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate)), Times.Once);

			Assert.IsTrue(communication.Success);
			Assert.IsInstanceOfType(response, typeof(AuthenticationResponse));
		}

		[TestMethod]
		public void MustIndicateIfAuthenticationCommandNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate))).Returns<Response>(null);

			var communication = sut.RequestAuthentication();

			Assert.AreEqual(default(AuthenticationResponse), communication.Value);
			Assert.IsFalse(communication.Success);
		}
	}
}
