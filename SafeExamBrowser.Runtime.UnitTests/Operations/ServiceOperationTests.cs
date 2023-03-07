/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ServiceOperationTests
	{
		private SessionContext context;
		private Mock<ILogger> logger;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IServiceProxy> service;
		private EventWaitHandle serviceEvent;
		private Mock<IUserInfo> userInfo;
		private ServiceOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			var serviceEventName = $"{nameof(SafeExamBrowser)}-{nameof(ServiceOperationTests)}";

			logger = new Mock<ILogger>();
			runtimeHost = new Mock<IRuntimeHost>();
			service = new Mock<IServiceProxy>();
			serviceEvent = new EventWaitHandle(false, EventResetMode.AutoReset, serviceEventName);
			context = new SessionContext();
			userInfo = new Mock<IUserInfo>();

			context.Current = new SessionConfiguration();
			context.Current.AppConfig = new AppConfig();
			context.Current.AppConfig.ServiceEventName = serviceEventName;
			context.Current.Settings = new AppSettings();
			context.Next = new SessionConfiguration();
			context.Next.AppConfig = new AppConfig();
			context.Next.AppConfig.ServiceEventName = serviceEventName;
			context.Next.Settings = new AppSettings();

			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, context, 0, userInfo.Object);
		}

		[TestMethod]
		public void Perform_MustConnectToService()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;

			sut.Perform();

			service.Setup(s => s.Connect(null, true)).Returns(true);
			context.Next.Settings.Service.Policy = ServicePolicy.Optional;

			sut.Perform();

			service.Verify(s => s.Connect(null, true), Times.Exactly(2));
		}

		[TestMethod]
		public void Perform_MustNotConnectToServiceWithIgnoreSet()
		{
			service.Setup(s => s.Connect(null, true)).Returns(true);
			context.Next.Settings.Service.IgnoreService = true;
			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;

			sut.Perform();

			service.Setup(s => s.Connect(null, true)).Returns(true);
			context.Next.Settings.Service.Policy = ServicePolicy.Optional;

			sut.Perform();

			service.Verify(s => s.Connect(null, true), Times.Never);
		}

		[TestMethod]
		public void Perform_MustStartSessionIfConnected()
		{
			context.Next.SessionId = Guid.NewGuid();
			context.Next.Settings.Service.Policy = ServicePolicy.Optional;
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.Is<ServiceConfiguration>(c => c.SessionId == context.Next.SessionId)), Times.Once);
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Never);
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

			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, context, TIMEOUT, userInfo.Object);

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
			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
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

			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);
			sut.ActionRequired += (args) => errorShown = args is MessageEventArgs m && m.Icon == MessageBoxIcon.Error;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(errorShown);
		}

		[TestMethod]
		public void Perform_MustNotFailIfServiceOptionalAndNotAvailable()
		{
			context.Next.Settings.Service.Policy = ServicePolicy.Optional;
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);

			var result = sut.Perform();

			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustShowWarningIfServiceNotAvailableAndPolicyWarn()
		{
			var warningShown = false;

			context.Next.Settings.Service.Policy = ServicePolicy.Warn;
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(false);
			sut.ActionRequired += (args) => warningShown = args is MessageEventArgs m && m.Icon == MessageBoxIcon.Warning;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(warningShown);
		}

		[TestMethod]
		public void Repeat_MustStopCurrentAndStartNewSession()
		{
			var order = 0;
			var start1 = 0;
			var stop1 = 0;
			var start2 = 0;
			var session1Id = Guid.NewGuid();
			var session2Id = Guid.NewGuid();

			context.Next.SessionId = session1Id;
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.Connect(null, true)).Returns(true);
			service
				.Setup(s => s.StartSession(It.Is<ServiceConfiguration>(c => c.SessionId == session1Id)))
				.Returns(new CommunicationResult(true))
				.Callback(() => { start1 = ++order; serviceEvent.Set(); });
			service
				.Setup(s => s.StartSession(It.Is<ServiceConfiguration>(c => c.SessionId == session2Id)))
				.Returns(new CommunicationResult(true))
				.Callback(() => { start2 = ++order; serviceEvent.Set(); });
			service
				.Setup(s => s.StopSession(It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(() => { stop1 = ++order; serviceEvent.Set(); });

			sut.Perform();

			context.Current.SessionId = session1Id;
			context.Next.SessionId = session2Id;

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Once);
			service.Verify(s => s.StopSession(It.Is<Guid>(id => id == session1Id)), Times.Once);
			service.Verify(s => s.StartSession(It.Is<ServiceConfiguration>(c => c.SessionId == session1Id)), Times.Once);
			service.Verify(s => s.StartSession(It.Is<ServiceConfiguration>(c => c.SessionId == session2Id)), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Never);
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(start1 == 1);
			Assert.IsTrue(stop1 == 2);
			Assert.IsTrue(start2 == 3);
		}

		[TestMethod]
		public void Repeat_MustEstablishConnectionIfNotConnected()
		{
			PerformNormally();

			service.Reset();
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(true).Callback(() => service.SetupGet(s => s.IsConnected).Returns(true));
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Once);
			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustNotEstablishConnectionWithIgnoreSet()
		{
			PerformNormally();

			context.Next.Settings.Service.IgnoreService = true;
			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;

			service.Reset();
			service.SetupGet(s => s.IsConnected).Returns(false);
			service.Setup(s => s.Connect(null, true)).Returns(true).Callback(() => service.SetupGet(s => s.IsConnected).Returns(true));
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Never);
			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustStopSessionAndCloseConnectionWithIgnoreSet()
		{
			var connected = true;

			PerformNormally();

			context.Next.Settings.Service.IgnoreService = true;
			context.Next.Settings.Service.Policy = ServicePolicy.Mandatory;

			service.Reset();
			service.SetupGet(s => s.IsConnected).Returns(() => connected);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());
			service.Setup(s => s.Disconnect()).Returns(true).Callback(() => connected = false);

			var result = sut.Repeat();

			service.Verify(s => s.Connect(It.IsAny<Guid?>(), It.IsAny<bool>()), Times.Never);
			service.Verify(s => s.StartSession(It.IsAny<ServiceConfiguration>()), Times.Never);
			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);

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
			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, context, TIMEOUT, userInfo.Object);

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
		public void Repeat_MustNotStopSessionIfNoSessionRunning()
		{
			service.SetupGet(s => s.IsConnected).Returns(true);
			service.Setup(s => s.StartSession(It.IsAny<ServiceConfiguration>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());

			var result = sut.Repeat();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDisconnect()
		{
			service.Setup(s => s.Disconnect()).Returns(true);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());
			service.Setup(s => s.RunSystemConfigurationUpdate()).Returns(new CommunicationResult(true));

			PerformNormally();

			var result = sut.Revert();

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
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustHandleCommunicationFailureWhenInitiatingSystemConfigurationUpdate()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());
			service.Setup(s => s.RunSystemConfigurationUpdate()).Returns(new CommunicationResult(false));

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustFailIfSessionStopUnsuccessful()
		{
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustFailIfSessionNotStoppedWithinTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);

			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true));
			sut = new ServiceOperation(logger.Object, runtimeHost.Object, service.Object, context, TIMEOUT, userInfo.Object);

			PerformNormally();

			before = DateTime.Now;
			var result = sut.Revert();
			after = DateTime.Now;

			service.Verify(s => s.StopSession(It.IsAny<Guid>()), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Never);

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
			context.Current = null;
			context.Next = null;
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

		[TestMethod]
		public void Revert_MustStopSessionIfConnected()
		{
			context.Next.SessionId = Guid.NewGuid();
			service.Setup(s => s.Disconnect()).Returns(true);
			service.Setup(s => s.StopSession(It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());
			service.Setup(s => s.RunSystemConfigurationUpdate()).Returns(new CommunicationResult(true));

			PerformNormally();

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.Is<Guid>(id => id == context.Next.SessionId)), Times.Once);
			service.Verify(s => s.Disconnect(), Times.Once);
			service.Verify(s => s.RunSystemConfigurationUpdate(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustStopSessionIfNewSessionNotCompletelyStarted()
		{
			var sessionId = Guid.NewGuid();

			context.Current.SessionId = Guid.NewGuid();
			context.Next.SessionId = sessionId;

			PerformNormally();

			context.Current.SessionId = default(Guid);
			context.Next.SessionId = default(Guid);
			service.Setup(s => s.StopSession(It.Is<Guid>(id => id == sessionId))).Returns(new CommunicationResult(true)).Callback(() => serviceEvent.Set());
			service.Setup(s => s.RunSystemConfigurationUpdate()).Returns(new CommunicationResult(true));
			service.Setup(s => s.Disconnect()).Returns(true);

			var result = sut.Revert();

			service.Verify(s => s.StopSession(It.Is<Guid>(id => id == sessionId)), Times.Once);
			service.Verify(s => s.StopSession(It.Is<Guid>(id => id == default(Guid))), Times.Never);
			service.Verify(s => s.Disconnect(), Times.Once);

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
