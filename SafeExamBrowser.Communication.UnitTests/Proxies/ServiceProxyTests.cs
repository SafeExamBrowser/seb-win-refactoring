/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.UnitTests.Proxies
{
	[TestClass]
	public class ServiceProxyTests
	{
		private Mock<ILogger> logger;
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<IProxyObject> proxy;
		private ServiceProxy sut;

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

			proxy.Setup(p => p.Connect(null)).Returns(response);
			proxy.Setup(o => o.State).Returns(System.ServiceModel.CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			sut = new ServiceProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object, default(Interlocutor));
			sut.Connect();
		}

		[TestMethod]
		public void MustCorrectlySendSystemConfigurationUpdate()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.UpdateSystemConfiguration))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RunSystemConfigurationUpdate();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.UpdateSystemConfiguration)), Times.Once);

			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfSystemConfigurationUpdateNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.UpdateSystemConfiguration))).Returns<Response>(null);

			var communication = sut.RunSystemConfigurationUpdate();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyStartSession()
		{
			var configuration = new ServiceConfiguration { SessionId = Guid.NewGuid() };

			proxy.Setup(p => p.Send(It.IsAny<SessionStartMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.StartSession(configuration);

			proxy.Verify(p => p.Send(It.Is<SessionStartMessage>(m => m.Configuration.SessionId == configuration.SessionId)), Times.Once);

			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfSessionStartNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<SessionStartMessage>())).Returns<Response>(null);

			var communication = sut.StartSession(new ServiceConfiguration());

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyStopSession()
		{
			var sessionId = Guid.NewGuid();

			proxy.Setup(p => p.Send(It.IsAny<SessionStopMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.StopSession(sessionId);

			proxy.Verify(p => p.Send(It.Is<SessionStopMessage>(m => m.SessionId == sessionId)), Times.Once);

			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfSessionStopNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<SessionStopMessage>())).Returns<Response>(null);

			var communication = sut.StopSession(Guid.Empty);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustExecuteOperationsFailsafe()
		{
			proxy.Setup(p => p.Send(It.IsAny<Message>())).Throws<Exception>();

			var configuration = sut.RunSystemConfigurationUpdate();
			var start = sut.StartSession(default(ServiceConfiguration));
			var stop = sut.StopSession(default(Guid));

			Assert.IsFalse(configuration.Success);
			Assert.IsFalse(start.Success);
			Assert.IsFalse(stop.Success);
		}
	}
}
