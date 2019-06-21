/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ServiceOperationTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IServiceProxy> service;
		private EventWaitHandle serviceEvent;
		private SessionConfiguration session;
		private SessionContext sessionContext;
		private Settings settings;
		private Mock<IUserInfo> userInfo;
		private ServiceOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			var serviceEventName = $"{nameof(SafeExamBrowser)}-{nameof(ServiceOperationTests)}";

			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			runtimeHost = new Mock<IRuntimeHost>();
			service = new Mock<IServiceProxy>();
			serviceEvent = new EventWaitHandle(false, EventResetMode.AutoReset, serviceEventName);
			session = new SessionConfiguration();
			sessionContext = new SessionContext();
			settings = new Settings();
			userInfo = new Mock<IUserInfo>();

			appConfig.ServiceEventName = serviceEventName;
			sessionContext.Current = session;
			sessionContext.Current.AppConfig = appConfig;
			sessionContext.Next = session;
			sessionContext.Next.AppConfig = appConfig;
			session.Settings = settings;
			settings.Service.Policy = ServicePolicy.Mandatory;

			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, sessionContext, 0, userInfo.Object);
		}

		[TestMethod]
		public void Perform_MustConnectToService()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.Service.Policy = ServicePolicy.Mandatory;

			sut.Perform();

			service.Setup(s => s.Connect(null, true)).Returns(true);
			settings.Service.Policy = ServicePolicy.Optional;

			sut.Perform();

			service.Verify(s => s.Connect(null, true), Times.Exactly(2));
		}

		[TestMethod]
		public void Perform_MustStartSessionIfConnected()
		{
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Once);
			userInfo.Verify(u => u.GetUserName(), Times.Once);
			userInfo.Verify(u => u.GetUserSid(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailIfSessionStartUnsuccessful()
		{
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true));

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustFailIfSessionNotStartedWithinTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);

			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true));

			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, sessionContext, TIMEOUT, userInfo.Object);

			before = DateTime.Now;
			var result = sut.Perform();
			after = DateTime.Now;

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, TIMEOUT));
		}

		[TestMethod]
		public void Perform_MustNotStartSessionIfNotConnected()
		{
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);

			sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Never);
		}

		[TestMethod]
		public void Perform_MustHandleCommunicationFailureWhenStartingSession()
		{
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(false));

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustFailIfServiceMandatoryAndNotAvailable()
		{
			var errorShown = false;

			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.Service.Policy = ServicePolicy.Mandatory;
			sut.ActionRequired += (args) => errorShown = args is MessageEventArgs m && m.Icon == MessageBoxIcon.Error;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(errorShown);
		}

		[TestMethod]
		public void Perform_MustNotFailIfServiceOptionalAndNotAvailable()
		{
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.Service.Policy = ServicePolicy.Optional;

			var result = sut.Perform();

			service.VerifySet(s => s.Ignore = true);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustShowWarningIfServiceNotAvailableAndPolicyWarn()
		{
			var warningShown = false;

			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);
			settings.Service.Policy = ServicePolicy.Warn;
			sut.ActionRequired += (args) => warningShown = args is MessageEventArgs m && m.Icon == MessageBoxIcon.Warning;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(warningShown);
		}

		[TestMethod]
		public void Repeat_MustStopCurrentAndStartNewSession()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			PerformNormally();

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Once);
			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Exactly(2));
			service.Verify(s => s.Disconnect(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustEstablishConnectionIfNotConnected()
		{
			PerformNormally();

			service.Reset();
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustFailIfCurrentSessionWasNotStoppedSuccessfully()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(false));

			PerformNormally();

			var result = sut.Repeat();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustFailIfSessionNotStoppedWithinTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);

			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));
			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, sessionContext, TIMEOUT, userInfo.Object);

			PerformNormally();

			before = DateTime.Now;
			var result = sut.Repeat();
			after = DateTime.Now;

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, TIMEOUT));
		}

		[TestMethod]
		public void Revert_MustDisconnect()
		{
			service.Setup(s => s.Disconnect()).Returns(true);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustStopSessionIfConnected()
		{
			service.Setup(s => s.Disconnect()).Returns(true);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustHandleCommunicationFailureWhenStoppingSession()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(false));

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustFailIfSessionStopUnsuccessful()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));

			PerformNormally();

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustFailIfSessionNotStoppedWithinTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);

			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));
			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, sessionContext, TIMEOUT, userInfo.Object);

			PerformNormally();

			before = DateTime.Now;
			var result = sut.Revert();
			after = DateTime.Now;

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, TIMEOUT));
		}

		[TestMethod]
		public void Revert_MustNotStopSessionWhenNotConnected()
		{
			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Never);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustNotStopSessionIfNoSessionRunning()
		{
			sessionContext.Current = null;
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Disconnect()).Returns(true);

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustNotDisconnnectIfNotConnected()
		{
			var result = sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Never);
			Assert.AreEqual(OperationResult.Success, result);
		}

		private void PerformNormally()
		{
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			sut.Perform();
		}
	}
}
