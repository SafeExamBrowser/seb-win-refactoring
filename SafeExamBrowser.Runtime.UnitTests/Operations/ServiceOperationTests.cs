/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ServiceOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IServiceProxy> service;
		private Mock<ISessionConfiguration> session;
		private SessionContext sessionContext;
		private Settings settings;
		private ServiceOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			service = new Mock<IServiceProxy>();
			session = new Mock<ISessionConfiguration>();
			sessionContext = new SessionContext();
			settings = new Settings();

			sessionContext.Current = session.Object;
			sessionContext.Next = session.Object;
			session.SetupGet(s => s.Settings).Returns(settings);

			sut = new ServiceOperation(logger.Object, service.Object, sessionContext);
		}

		[TestMethod]
		public void MustConnectToService()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.ServicePolicy = ServicePolicy.Mandatory;

			sut.Perform();

			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.ServicePolicy = ServicePolicy.Optional;

			sut.Perform();

			service.Verify(s => s.Connect(null, true), Times.Exactly(2));
		}

		[TestMethod]
		public void MustStartSessionIfConnected()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);

			sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<Guid>(), It.IsAny<Settings>()), Times.Once);
		}

		[TestMethod]
		public void MustNotStartSessionIfNotConnected()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);

			sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<Guid>(), It.IsAny<Settings>()), Times.Never);
		}

		[TestMethod]
		public void MustNotFailIfServiceNotAvailable()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Mandatory;

			sut.Perform();

			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Optional;

			sut.Perform();
		}

		[TestMethod]
		public void MustFailIfServiceMandatoryAndNotAvailable()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Mandatory;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustNotFailIfServiceOptionalAndNotAvailable()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Optional;

			var result = sut.Perform();

			service.VerifySet(s => s.Ignore = true);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDisconnectWhenReverting()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.ServicePolicy = ServicePolicy.Mandatory;

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.ServicePolicy = ServicePolicy.Optional;

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustStopSessionWhenReverting()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
		}

		[TestMethod]
		public void MustNotStopSessionWhenRevertingAndNotConnected()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Never);
		}

		[TestMethod]
		public void MustNotFailWhenDisconnecting()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.Disconnect()).Returns(false);
			settings.ServicePolicy = ServicePolicy.Optional;

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Once);
		}

		[TestMethod]
		public void MustNotDisconnnectIfNotAvailable()
		{
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Mandatory;

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.ServicePolicy = ServicePolicy.Optional;

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Never);
		}
	}
}
