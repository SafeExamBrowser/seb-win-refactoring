/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class ClientControllerTests
	{
		private AppConfig appConfig;
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<IOperationSequence> operationSequence;
		private Mock<IResponsibilityCollection<ClientTask>> responsibilities;
		private Mock<IRuntimeProxy> runtimeProxy;
		private Mock<ISplashScreen> splashScreen;

		private ClientController sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			context = new ClientContext();
			logger = new Mock<ILogger>();
			operationSequence = new Mock<IOperationSequence>();
			responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();
			runtimeProxy = new Mock<IRuntimeProxy>();
			splashScreen = new Mock<ISplashScreen>();

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			operationSequence.Setup(o => o.TryRevert()).Returns(OperationResult.Success);
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true));

			sut = new ClientController(
				context,
				logger.Object,
				operationSequence.Object,
				responsibilities.Object,
				runtimeProxy.Object,
				splashScreen.Object);

			context.AppConfig = appConfig;
		}

		[TestMethod]
		public void Shutdown_MustDelegateResponsibilities()
		{
			var order = 0;

			responsibilities.Setup(r => r.Delegate(ClientTask.CloseShell)).Callback(() => Assert.AreEqual(1, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.DeregisterEvents)).Callback(() => Assert.AreEqual(2, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.UpdateSessionIntegrity)).Callback(() => Assert.AreEqual(3, ++order));

			sut.Terminate();

			responsibilities.Verify(r => r.Delegate(ClientTask.CloseShell), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.DeregisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.UpdateSessionIntegrity), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustRevertOperations()
		{
			sut.Terminate();

			operationSequence.Verify(o => o.TryRevert(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustDelegateResponsibilities()
		{
			var clientReady = int.MaxValue;
			var order = 0;

			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true)).Callback(() => clientReady = order);

			// Startup
			responsibilities.Setup(r => r.Delegate(ClientTask.RegisterEvents)).Callback(() => Assert.AreEqual(1, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.ShowShell)).Callback(() => Assert.AreEqual(2, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.AutoStartApplications)).Callback(() => Assert.AreEqual(3, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.ScheduleIntegrityVerification)).Callback(() => Assert.AreEqual(4, ++order));
			responsibilities.Setup(r => r.Delegate(ClientTask.StartMonitoring)).Callback(() => Assert.AreEqual(5, ++order));

			// Client Ready
			responsibilities.Setup(r => r.Delegate(ClientTask.VerifySessionIntegrity)).Callback(() => Assert.IsTrue(6 == ++order && clientReady < order));

			sut.TryStart();

			responsibilities.Verify(r => r.Delegate(ClientTask.RegisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.ShowShell), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.AutoStartApplications), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.ScheduleIntegrityVerification), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.StartMonitoring), Times.Once);
			responsibilities.Verify(r => r.Delegate(ClientTask.VerifySessionIntegrity), Times.Once);
		}

		[TestMethod]
		public void Startup_MustNotDelegateStartupResponsibilitiesOnFailure()
		{
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Failed);

			sut.TryStart();

			responsibilities.Verify(r => r.Delegate(ClientTask.RegisterEvents), Times.Never);
			responsibilities.Verify(r => r.Delegate(ClientTask.ShowShell), Times.Never);
			responsibilities.Verify(r => r.Delegate(ClientTask.AutoStartApplications), Times.Never);
			responsibilities.Verify(r => r.Delegate(ClientTask.ScheduleIntegrityVerification), Times.Never);
			responsibilities.Verify(r => r.Delegate(ClientTask.StartMonitoring), Times.Never);
		}

		[TestMethod]
		public void Startup_MustNotDelegateClientReadyResponsibilitiesOnFailure()
		{
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(false));

			sut.TryStart();

			responsibilities.Verify(r => r.Delegate(ClientTask.VerifySessionIntegrity), Times.Never);
		}

		[TestMethod]
		public void Startup_MustInformRuntime()
		{
			sut.TryStart();

			runtimeProxy.Verify(r => r.InformClientReady(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustPerformOperations()
		{
			sut.TryStart();

			operationSequence.Verify(o => o.TryPerform(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustSucceed()
		{
			var success = sut.TryStart();

			Assert.IsTrue(success);
		}

		[TestMethod]
		public void Startup_MustHandleCommunicationError()
		{
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(false));

			var success = sut.TryStart();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void Startup_MustHandleFailure()
		{
			var success = true;

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Failed);
			success = sut.TryStart();

			Assert.IsFalse(success);

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Aborted);
			success = sut.TryStart();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void Startup_MustUpdateAppConfigForSplashScreen()
		{
			sut.UpdateAppConfig();
			splashScreen.VerifySet(s => s.AppConfig = appConfig, Times.Once);
		}
	}
}
