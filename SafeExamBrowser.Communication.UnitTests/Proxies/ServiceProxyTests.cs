/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Communication.Proxies;

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

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			sut = new ServiceProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object);
		}

		[TestMethod]
		public void MustIgnoreConnectIfUnavailable()
		{
			sut.Ignore = true;
			sut.Connect(Guid.NewGuid());

			proxy.Verify(p => p.Connect(It.IsAny<Guid>()), Times.Never);
		}

		[TestMethod]
		public void MustIgnoreDisconnectIfUnavailable()
		{
			sut.Ignore = true;
			sut.Disconnect();

			proxy.Verify(p => p.Disconnect(It.IsAny<DisconnectionMessage>()), Times.Never);
		}

		[TestMethod]
		public void MustIgnoreStartSessionIfUnavaiable()
		{
			sut.Ignore = true;

			var communication = sut.StartSession(null);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.IsAny<Message>()), Times.Never);
		}

		[TestMethod]
		public void MustIgnoreStopSessionIfUnavaiable()
		{
			sut.Ignore = true;

			var communication = sut.StopSession(Guid.Empty);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.IsAny<Message>()), Times.Never);
		}
	}
}
