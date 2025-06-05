/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class RuntimeControllerTests
	{
		private Mock<IOperationSequence> bootstrapSequence;
		private RuntimeContext context;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<ISplashScreen> splashScreen;

		private RuntimeController sut;

		[TestInitialize]
		public void Initialize()
		{
			bootstrapSequence = new Mock<IOperationSequence>();
			context = new RuntimeContext();
			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			splashScreen = new Mock<ISplashScreen>();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;

			context.Current = currentSession;
			context.Next = nextSession;

			sut = new RuntimeController(logger.Object, bootstrapSequence.Object, responsibilities.Object, context, runtimeWindow.Object, splashScreen.Object);
		}

		[TestMethod]
		public void Shutdown_MustStopSessionThenBootstrapSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			responsibilities.Setup(r => r.Delegate(RuntimeTask.StopSession)).Callback(() => session = ++order);

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.DeregisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StopSession), Times.Once);

			Assert.AreEqual(1, session);
			Assert.AreEqual(2, bootstrap);
		}

		[TestMethod]
		public void Shutdown_MustOnlyRevertBootstrapSequenceIfNoSessionRunning()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			context.Current = default;
			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			responsibilities.Setup(r => r.Delegate(RuntimeTask.StopSession)).Callback(() => session = ++order);

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.DeregisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StopSession), Times.Never);

			Assert.AreEqual(0, session);
			Assert.AreEqual(1, bootstrap);
		}

		[TestMethod]
		public void Startup_MustPerformBootstrapThenSessionSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			context.Current = default;
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			responsibilities.Setup(r => r.Delegate(RuntimeTask.StartSession)).Callback(() => session = ++order);

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.RegisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StartSession), Times.Once);

			Assert.IsTrue(success);
			Assert.AreEqual(1, bootstrap);
			Assert.AreEqual(2, session);
		}

		[TestMethod]
		public void Startup_MustNotStartSessionIfBootstrapSequenceFails()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			context.Current = default;
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Failed).Callback(() => bootstrap = ++order);
			responsibilities.Setup(r => r.Delegate(RuntimeTask.StartSession)).Callback(() => session = ++order);

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.RegisterEvents), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StartSession), Times.Never);

			Assert.IsFalse(success);
			Assert.AreEqual(1, bootstrap);
			Assert.AreEqual(0, session);
		}
	}
}
