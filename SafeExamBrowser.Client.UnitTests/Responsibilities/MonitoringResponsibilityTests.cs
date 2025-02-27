/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.Settings.UserInterface;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.WindowsApi.Contracts;
using IWindow = SafeExamBrowser.UserInterface.Contracts.Windows.IWindow;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class MonitoringResponsibilityTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IApplicationMonitor> applicationMonitor;
		private ClientContext context;
		private Mock<ICoordinator> coordinator;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<IExplorerShell> explorerShell;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<IMessageBox> messageBox;
		private Mock<IRuntimeProxy> runtime;
		private Mock<ISystemSentinel> sentinel;
		private AppSettings settings;
		private Mock<ITaskbar> taskbar;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private MonitoringResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();
			var responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();

			actionCenter = new Mock<IActionCenter>();
			applicationMonitor = new Mock<IApplicationMonitor>();
			context = new ClientContext();
			coordinator = new Mock<ICoordinator>();
			displayMonitor = new Mock<IDisplayMonitor>();
			explorerShell = new Mock<IExplorerShell>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			messageBox = new Mock<IMessageBox>();
			runtime = new Mock<IRuntimeProxy>();
			sentinel = new Mock<ISystemSentinel>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.HashAlgorithm = hashAlgorithm.Object;
			context.MessageBox = messageBox.Object;
			context.Responsibilities = responsibilities.Object;
			context.Runtime = runtime.Object;
			context.Settings = settings;
			context.UserInterfaceFactory = uiFactory.Object;

			sut = new MonitoringResponsibility(
				actionCenter.Object,
				applicationMonitor.Object,
				context,
				coordinator.Object,
				displayMonitor.Object,
				explorerShell.Object,
				logger.Object,
				sentinel.Object,
				taskbar.Object,
				text.Object);

			sut.Assume(ClientTask.RegisterEvents);
			sut.Assume(ClientTask.StartMonitoring);
		}

		[TestMethod]
		public void ApplicationMonitor_MustCorrectlyHandleExplorerStartWithTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 30;
			var order = 0;
			var shell = 0;
			var workingArea = 0;

			settings.UserInterface.Taskbar.EnableTaskbar = true;

			actionCenter.Setup(a => a.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			explorerShell.Setup(e => e.Terminate()).Callback(() => shell = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == height))).Callback(() => workingArea = ++order);
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			applicationMonitor.Raise(a => a.ExplorerStarted += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			explorerShell.Verify(e => e.Terminate(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Never);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Once);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Once);

			Assert.IsTrue(shell == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(boundsActionCenter == 3);
			Assert.IsTrue(boundsTaskbar == 4);
		}

		[TestMethod]
		public void ApplicationMonitor_MustCorrectlyHandleExplorerStartWithoutTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 30;
			var order = 0;
			var shell = 0;
			var workingArea = 0;

			settings.UserInterface.Taskbar.EnableTaskbar = false;

			actionCenter.Setup(a => a.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			explorerShell.Setup(e => e.Terminate()).Callback(() => shell = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == 0))).Callback(() => workingArea = ++order);
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			applicationMonitor.Raise(a => a.ExplorerStarted += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			explorerShell.Verify(e => e.Terminate(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Never);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Never);

			Assert.IsTrue(shell == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(boundsActionCenter == 3);
			Assert.IsTrue(boundsTaskbar == 4);
		}

		[TestMethod]
		public void ApplicationMonitor_MustPermitApplicationIfChosenByUserAfterFailedTermination()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtime.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object)
				.Callback<string, string, IEnumerable<LockScreenOption>, LockScreenSettings>((m, t, o, s) => result.OptionId = o.First().Id);

			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			runtime.Verify(p => p.RequestShutdown(), Times.Never);
		}

		[TestMethod]
		public void ApplicationMonitor_MustRequestShutdownIfChosenByUserAfterFailedTermination()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtime.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object)
				.Callback<string, string, IEnumerable<LockScreenOption>, LockScreenSettings>((m, t, o, s) => result.OptionId = o.Last().Id);

			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			runtime.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void ApplicationMonitor_MustShowLockScreenIfTerminationFailed()
		{
			var activator1 = new Mock<IActivator>();
			var activator2 = new Mock<IActivator>();
			var activator3 = new Mock<IActivator>();
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();
			var order = 0;
			var pause = 0;
			var show = 0;
			var wait = 0;
			var close = 0;
			var resume = 0;

			activator1.Setup(a => a.Pause()).Callback(() => pause = ++order);
			activator1.Setup(a => a.Resume()).Callback(() => resume = ++order);
			context.Activators.Add(activator1.Object);
			context.Activators.Add(activator2.Object);
			context.Activators.Add(activator3.Object);
			lockScreen.Setup(l => l.Show()).Callback(() => show = ++order);
			lockScreen.Setup(l => l.WaitForResult()).Callback(() => wait = ++order).Returns(result);
			lockScreen.Setup(l => l.Close()).Callback(() => close = ++order);
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object);

			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			activator1.Verify(a => a.Pause(), Times.Once);
			activator1.Verify(a => a.Resume(), Times.Once);
			activator2.Verify(a => a.Pause(), Times.Once);
			activator2.Verify(a => a.Resume(), Times.Once);
			activator3.Verify(a => a.Pause(), Times.Once);
			activator3.Verify(a => a.Resume(), Times.Once);
			lockScreen.Verify(l => l.Show(), Times.Once);
			lockScreen.Verify(l => l.WaitForResult(), Times.Once);
			lockScreen.Verify(l => l.Close(), Times.Once);

			Assert.IsTrue(pause == 1);
			Assert.IsTrue(show == 2);
			Assert.IsTrue(wait == 3);
			Assert.IsTrue(close == 4);
			Assert.IsTrue(resume == 5);
		}

		[TestMethod]
		public void ApplicationMonitor_MustValidateQuitPasswordIfTerminationFailed()
		{
			var hash = "12345";
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult { Password = "test" };
			var attempt = 0;
			var correct = new Random().Next(1, 50);
			var lockScreenResult = new Func<LockScreenResult>(() => ++attempt == correct ? result : new LockScreenResult());

			context.Settings.Security.QuitPasswordHash = hash;
			hashAlgorithm.Setup(a => a.GenerateHashFor(It.Is<string>(p => p == result.Password))).Returns(hash);
			lockScreen.Setup(l => l.WaitForResult()).Returns(lockScreenResult);
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object);

			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			hashAlgorithm.Verify(a => a.GenerateHashFor(It.Is<string>(p => p == result.Password)), Times.Once);
			hashAlgorithm.Verify(a => a.GenerateHashFor(It.Is<string>(p => p != result.Password)), Times.Exactly(attempt - 1));
			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(w => w == lockScreen.Object)), Times.Exactly(attempt - 1));
		}

		[TestMethod]
		public void DisplayMonitor_MustCorrectlyHandleDisplayChangeWithTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 25;
			var order = 0;
			var workingArea = 0;

			settings.UserInterface.Taskbar.EnableTaskbar = true;

			actionCenter.Setup(t => t.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			displayMonitor.Setup(m => m.InitializePrimaryDisplay(It.Is<int>(h => h == height))).Callback(() => workingArea = ++order);
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			displayMonitor.Raise(d => d.DisplayChanged += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Never);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Once);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(workingArea == 1);
			Assert.IsTrue(boundsActionCenter == 2);
			Assert.IsTrue(boundsTaskbar == 3);
		}

		[TestMethod]
		public void DisplayMonitor_MustCorrectlyHandleDisplayChangeWithoutTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 25;
			var order = 0;
			var workingArea = 0;

			settings.UserInterface.Taskbar.EnableTaskbar = false;

			actionCenter.Setup(t => t.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == 0))).Callback(() => workingArea = ++order);
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			displayMonitor.Raise(d => d.DisplayChanged += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Never);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Never);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(workingArea == 1);
			Assert.IsTrue(boundsActionCenter == 2);
			Assert.IsTrue(boundsTaskbar == 3);
		}

		[TestMethod]
		public void DisplayMonitor_MustShowLockScreenOnDisplayChange()
		{
			var lockScreen = new Mock<ILockScreen>();

			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = false });
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object);

			displayMonitor.Raise(d => d.DisplayChanged += null);

			lockScreen.Verify(l => l.Show(), Times.Once);
		}

		[TestMethod]
		public void SystemMonitor_MustShowLockScreenOnSessionSwitch()
		{
			var lockScreen = new Mock<ILockScreen>();

			coordinator.Setup(c => c.RequestSessionLock()).Returns(true);
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			settings.Service.IgnoreService = true;
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object);

			sentinel.Raise(s => s.SessionChanged += null);

			coordinator.Verify(c => c.RequestSessionLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseSessionLock(), Times.Once);
			lockScreen.Verify(l => l.Show(), Times.Once);
		}

		[TestMethod]
		public void SystemMonitor_MustTerminateIfRequestedByUser()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			coordinator.Setup(c => c.RequestSessionLock()).Returns(true);
			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			settings.Service.IgnoreService = true;
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Callback(new Action<string, string, IEnumerable<LockScreenOption>, LockScreenSettings>((message, title, options, settings) => result.OptionId = options.Last().Id))
				.Returns(lockScreen.Object);

			sentinel.Raise(s => s.SessionChanged += null);

			coordinator.Verify(c => c.RequestSessionLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseSessionLock(), Times.Once);
			lockScreen.Verify(l => l.Show(), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void SystemMonitor_MustDoNothingIfSessionSwitchAllowed()
		{
			var lockScreen = new Mock<ILockScreen>();

			settings.Service.IgnoreService = false;
			settings.Service.DisableUserLock = false;
			settings.Service.DisableUserSwitch = false;
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>(), It.IsAny<LockScreenSettings>()))
				.Returns(lockScreen.Object);

			sentinel.Raise(s => s.SessionChanged += null);

			lockScreen.Verify(l => l.Show(), Times.Never);
		}
	}
}
