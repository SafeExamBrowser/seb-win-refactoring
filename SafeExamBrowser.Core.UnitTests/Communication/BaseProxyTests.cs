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
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication;

namespace SafeExamBrowser.Core.UnitTests.Communication
{
	[TestClass]
	public class BaseProxyTests
	{
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<ILogger> logger;
		private BaseProxy sut;

		[TestInitialize]
		public void Initialize()
		{
			proxyObjectFactory = new Mock<IProxyObjectFactory>();
			logger = new Mock<ILogger>();

			sut = new BaseProxyImpl("net.pipe://some/address/here", proxyObjectFactory.Object, logger.Object);
		}

		[TestMethod]
		public void MustConnectCorrectly()
		{
			var proxy = new Mock<ICommunication>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			proxy.Verify(p => p.Connect(token), Times.Once);
			proxyObjectFactory.Verify(f => f.CreateObject(It.IsAny<string>()), Times.Once);

			Assert.IsTrue(connected);
		}

		[TestMethod]
		public void MustDisconnectCorrectly()
		{
			var proxy = new Mock<ICommunication>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};
			var disconnectionResponse = new DisconnectionResponse
			{
				ConnectionTerminated = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Disconnect(It.IsAny<DisconnectionMessage>())).Returns(disconnectionResponse);
			proxy.As<ICommunicationObject>().Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);
			var disconnected = sut.Disconnect();

			proxy.Verify(p => p.Disconnect(It.Is<DisconnectionMessage>(m => m.CommunicationToken == connectionResponse.CommunicationToken)), Times.Once);

			Assert.IsTrue(connected);
			Assert.IsTrue(disconnected);
		}

		[TestMethod]
		public void MustHandleConnectionRefusalCorrectly()
		{
			var proxy = new Mock<ICommunication>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = false
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			proxy.Verify(p => p.Connect(token), Times.Once);
			proxyObjectFactory.Verify(f => f.CreateObject(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(connected);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustFailToDisconnectIfNotConnected()
		{
			sut.Disconnect();
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustFailToDisconnectIfChannelNotOpen()
		{
			var proxy = new Mock<ICommunication>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.As<ICommunicationObject>().Setup(o => o.State).Returns(CommunicationState.Faulted);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();

			sut.Connect(token);
			sut.Disconnect();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustFailToSendIfNotConnected()
		{
			(sut as BaseProxyImpl).Send(new Mock<Message>().Object);
		}

		[TestMethod]
		[ExpectedException(typeof(CommunicationException))]
		public void MustFailToSendIfChannelNotOpen()
		{
			var proxy = new Mock<ICommunication>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.As<ICommunicationObject>().Setup(o => o.State).Returns(CommunicationState.Faulted);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();

			sut.Connect(token);
			(sut as BaseProxyImpl).Send(new Mock<Message>().Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowSendingNull()
		{
			(sut as BaseProxyImpl).Send(null);
		}

		[TestMethod]
		public void MustSendCorrectly()
		{
			var proxy = new Mock<ICommunication>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};
			var message = new SimpleMessage(SimpleMessagePurport.Authenticate);
			var response = new Mock<Response>();

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(message)).Returns(response.Object);
			proxy.As<ICommunicationObject>().Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);
			var received = (sut as BaseProxyImpl).Send(message);

			Assert.AreEqual(response.Object, received);
			Assert.AreEqual(connectionResponse.CommunicationToken, message.CommunicationToken);
		}
	}
}
