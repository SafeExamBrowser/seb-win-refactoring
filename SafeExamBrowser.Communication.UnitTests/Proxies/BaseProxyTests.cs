/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.UnitTests.Proxies
{
	[TestClass]
	public class BaseProxyTests
	{
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<ILogger> logger;
		private BaseProxyImpl sut;

		[TestInitialize]
		public void Initialize()
		{
			proxyObjectFactory = new Mock<IProxyObjectFactory>();
			logger = new Mock<ILogger>();

			sut = new BaseProxyImpl("net.pipe://some/address/here", proxyObjectFactory.Object, logger.Object, default(Interlocutor));
		}

		[TestMethod]
		public void MustConnectCorrectly()
		{
			var proxy = new Mock<IProxyObject>();
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
			var proxy = new Mock<IProxyObject>();
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
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
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
			var proxy = new Mock<IProxyObject>();
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
		public void MustHandleConnectionFailureCorrectly()
		{
			var proxy = new Mock<IProxyObject>();

			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Throws<Exception>();

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			proxyObjectFactory.Verify(f => f.CreateObject(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(connected);
		}

		[TestMethod]
		public void MustHandleMissingEndpointCorrectly()
		{
			var proxy = new Mock<IProxyObject>();

			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Throws<EndpointNotFoundException>();

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce());
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never());
			logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
			proxyObjectFactory.Verify(f => f.CreateObject(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(connected);
		}

		[TestMethod]
		public void MustFailToDisconnectIfNotConnected()
		{
			var success = sut.Disconnect();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void MustFailToDisconnectIfChannelNotOpen()
		{
			var proxy = new Mock<IProxyObject>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.Setup(o => o.State).Returns(CommunicationState.Faulted);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();

			sut.Connect(token);

			var success = sut.Disconnect();

			Assert.IsFalse(success);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustFailToSendIfNotConnected()
		{
			sut.Send(new Mock<Message>().Object);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustFailToSendIfChannelNotOpen()
		{
			var proxy = new Mock<IProxyObject>();
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.Setup(o => o.State).Returns(CommunicationState.Faulted);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();

			sut.Connect(token);
			sut.Send(new Mock<Message>().Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowSendingNull()
		{
			sut.Send(null);
		}

		[TestMethod]
		public void MustSendCorrectly()
		{
			var proxy = new Mock<IProxyObject>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};
			var message = new SimpleMessage(SimpleMessagePurport.Authenticate);
			var response = new Mock<Response>();

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(message)).Returns(response.Object);
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);
			var received = sut.Send(message);

			Assert.AreEqual(response.Object, received);
			Assert.AreEqual(connectionResponse.CommunicationToken, message.CommunicationToken);
		}

		[TestMethod]
		public void MustSendSimpleMessageCorrectly()
		{
			var proxy = new Mock<IProxyObject>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};
			var purport = SimpleMessagePurport.Authenticate;
			var response = new Mock<Response>();

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(It.IsAny<Message>())).Returns(response.Object);
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);
			var received = sut.Send(purport);

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == purport)));
		}

		[TestMethod]
		public void MustTestAcknowledgeResponsesCorrectly()
		{
			var nullResponse = sut.IsAcknowledged(null);
			var notAcknowledge = sut.IsAcknowledged(new SimpleResponse(SimpleResponsePurport.Unauthorized));
			var acknowledge = sut.IsAcknowledged(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			Assert.IsFalse(nullResponse);
			Assert.IsFalse(notAcknowledge);
			Assert.IsTrue(acknowledge);
		}

		[TestMethod]
		public void MustToStringSafely()
		{
			var message = new Mock<Message>();
			var response = new Mock<Response>();

			message.Setup(m => m.ToString()).Returns(nameof(Message));
			response.Setup(r => r.ToString()).Returns(nameof(Response));

			var nullStringMessage = sut.ToString(null as Message);
			var nullStringResponse = sut.ToString(null as Response);
			var messageString = sut.ToString(message.Object);
			var responseString = sut.ToString(response.Object);

			Assert.IsNotNull(nullStringMessage);
			Assert.IsNotNull(nullStringResponse);
			Assert.IsNotNull(messageString);
			Assert.IsNotNull(responseString);
			Assert.AreEqual(message.Object.ToString(), messageString);
			Assert.AreEqual(response.Object.ToString(), responseString);
		}

		[TestMethod]
		public void TestConnectionMustPingHost()
		{
			var proxy = new Mock<IProxyObject>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Ping))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			sut.TestConnection();

			proxy.Verify();
		}

		[TestMethod]
		public void TestConnectionMustInvokeConnectionLostEvent()
		{
			var lost = false;
			var proxy = new Mock<IProxyObject>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			sut.ConnectionLost += () => lost = true;

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Ping))).Returns(new SimpleResponse(SimpleResponsePurport.UnknownMessage));
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			sut.TestConnection();

			Assert.IsTrue(lost);
		}

		[TestMethod]
		public void TestConnectionMustNotFail()
		{
			var lost = false;
			var proxy = new Mock<IProxyObject>();
			var connectionResponse = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			sut.ConnectionLost += () => lost = true;

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(connectionResponse);
			proxy.Setup(p => p.Send(It.IsAny<Message>())).Throws<Exception>();
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			var token = Guid.NewGuid();
			var connected = sut.Connect(token);

			sut.TestConnection();

			Assert.IsTrue(lost);
		}

		[TestMethod]
		public void MustLogStatusChanges()
		{
			var proxy = new Mock<IProxyObject>();
			var connectionLost = false;

			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);
			sut.ConnectionLost += () => connectionLost = true;
			sut.Connect(Guid.Empty);

			proxy.Raise(p => p.Closed += null, It.IsAny<EventArgs>());
			proxy.Raise(p => p.Closing += null, It.IsAny<EventArgs>());
			proxy.Raise(p => p.Faulted += null, It.IsAny<EventArgs>());
			proxy.Raise(p => p.Opened += null, It.IsAny<EventArgs>());
			proxy.Raise(p => p.Opening += null, It.IsAny<EventArgs>());

			logger.Verify(l => l.Debug(It.IsAny<string>()), Times.AtLeast(4));
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce);

			Assert.IsTrue(connectionLost);
		}
	}
}
