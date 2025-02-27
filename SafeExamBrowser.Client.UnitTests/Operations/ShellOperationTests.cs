/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ShellOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IAudio> audio;
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<INotification> aboutNotification;
		private Mock<IKeyboard> keyboard;
		private Mock<INotification> logNotification;
		private Mock<INativeMethods> nativeMethods;
		private Mock<IPowerSupply> powerSupply;
		private Mock<ISystemInfo> systemInfo;
		private Mock<ITaskbar> taskbar;
		private Mock<ITaskview> taskview;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private Mock<INetworkAdapter> networkAdapter;

		private ShellOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			audio = new Mock<IAudio>();
			context = new ClientContext();
			logger = new Mock<ILogger>();
			aboutNotification = new Mock<INotification>();
			keyboard = new Mock<IKeyboard>();
			logNotification = new Mock<INotification>();
			nativeMethods = new Mock<INativeMethods>();
			networkAdapter = new Mock<INetworkAdapter>();
			powerSupply = new Mock<IPowerSupply>();
			systemInfo = new Mock<ISystemInfo>();
			taskbar = new Mock<ITaskbar>();
			taskview = new Mock<ITaskview>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Settings = new AppSettings();

			uiFactory
				.Setup(u => u.CreateNotificationControl(It.IsAny<INotification>(), It.IsAny<Location>()))
				.Returns(new Mock<INotificationControl>().Object);

			sut = new ShellOperation(
				actionCenter.Object,
				audio.Object,
				aboutNotification.Object,
				context,
				keyboard.Object,
				logger.Object,
				logNotification.Object,
				nativeMethods.Object,
				networkAdapter.Object,
				powerSupply.Object,
				systemInfo.Object,
				taskbar.Object,
				taskview.Object,
				text.Object,
				uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeActivators()
		{
			var actionCenterActivator = new Mock<IActionCenterActivator>();
			var taskviewActivator = new Mock<ITaskviewActivator>();
			var terminationActivator = new Mock<ITerminationActivator>();

			context.Activators.Add(actionCenterActivator.Object);
			context.Activators.Add(taskviewActivator.Object);
			context.Activators.Add(terminationActivator.Object);
			context.Settings.Keyboard.AllowAltTab = true;
			context.Settings.Security.AllowTermination = true;
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;

			sut.Perform();

			actionCenter.Verify(a => a.Register(It.Is<IActionCenterActivator>(a2 => a2 == actionCenterActivator.Object)), Times.Once);
			actionCenterActivator.Verify(a => a.Start(), Times.Once);
			taskview.Verify(t => t.Register(It.Is<ITaskviewActivator>(a => a == taskviewActivator.Object)), Times.Once);
			taskviewActivator.Verify(a => a.Start(), Times.Once);
			terminationActivator.Verify(a => a.Start(), Times.Once);
		}

		[TestMethod]
		public void Perform_MustInitializeApplications()
		{
			var application1 = new Mock<IApplication<IApplicationWindow>>();
			var application1Settings = new WhitelistApplication { ShowInShell = true };
			var application2 = new Mock<IApplication<IApplicationWindow>>();
			var application2Settings = new WhitelistApplication { ShowInShell = false };
			var application3 = new Mock<IApplication<IApplicationWindow>>();
			var application3Settings = new WhitelistApplication { ShowInShell = true };

			application1.SetupGet(a => a.Id).Returns(application1Settings.Id);
			application2.SetupGet(a => a.Id).Returns(application2Settings.Id);
			application3.SetupGet(a => a.Id).Returns(application3Settings.Id);

			context.Applications.Add(application1.Object);
			context.Applications.Add(application2.Object);
			context.Applications.Add(application3.Object);
			context.Settings.Applications.Whitelist.Add(application1Settings);
			context.Settings.Applications.Whitelist.Add(application2Settings);
			context.Settings.Applications.Whitelist.Add(application3Settings);
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;

			sut.Perform();

			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), false), Times.Exactly(2));
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), false), Times.Exactly(2));
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object)), Times.Once);
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object)), Times.Once);
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object)), Times.Once);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object), Location.ActionCenter), Times.Once);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object), Location.Taskbar), Times.Once);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object), Location.ActionCenter), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object), Location.Taskbar), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object), Location.ActionCenter), Times.Once);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object), Location.Taskbar), Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotAddApplicationsToShellIfNotEnabled()
		{
			var application1 = new Mock<IApplication<IApplicationWindow>>();
			var application1Settings = new WhitelistApplication { ShowInShell = true };
			var application2 = new Mock<IApplication<IApplicationWindow>>();
			var application2Settings = new WhitelistApplication { ShowInShell = true };
			var application3 = new Mock<IApplication<IApplicationWindow>>();
			var application3Settings = new WhitelistApplication { ShowInShell = true };

			application1.SetupGet(a => a.Id).Returns(application1Settings.Id);
			application2.SetupGet(a => a.Id).Returns(application2Settings.Id);
			application3.SetupGet(a => a.Id).Returns(application3Settings.Id);

			context.Applications.Add(application1.Object);
			context.Applications.Add(application2.Object);
			context.Applications.Add(application3.Object);
			context.Settings.Applications.Whitelist.Add(application1Settings);
			context.Settings.Applications.Whitelist.Add(application2Settings);
			context.Settings.Applications.Whitelist.Add(application3Settings);
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = false;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = false;

			sut.Perform();

			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), false), Times.Never);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), false), Times.Never);
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object)), Times.Once);
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object)), Times.Once);
			taskview.Verify(t => t.Add(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object)), Times.Once);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object), Location.ActionCenter), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application1.Object), Location.Taskbar), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object), Location.ActionCenter), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application2.Object), Location.Taskbar), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object), Location.ActionCenter), Times.Never);
			uiFactory.Verify(f => f.CreateApplicationControl(It.Is<IApplication<IApplicationWindow>>(a => a == application3.Object), Location.Taskbar), Times.Never);
		}

		[TestMethod]
		public void Perform_MustInitializeAlwaysOnState()
		{
			context.Settings.Display.AlwaysOn = true;
			context.Settings.System.AlwaysOn = false;

			sut.Perform();

			nativeMethods.Verify(n => n.SetAlwaysOnState(true, false), Times.Once);
			nativeMethods.Reset();

			context.Settings.Display.AlwaysOn = false;
			context.Settings.System.AlwaysOn = true;

			sut.Perform();

			nativeMethods.Verify(n => n.SetAlwaysOnState(false, true), Times.Once);
		}

		[TestMethod]
		public void Perform_MustInitializeClock()
		{
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowClock = true;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowClock = true;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowClock = true, Times.Once);
			taskbar.VerifySet(t => t.ShowClock = true, Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotInitializeClock()
		{
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowClock = false;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowClock = false;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowClock = false, Times.Once);
			taskbar.VerifySet(t => t.ShowClock = false, Times.Once);
		}

		[TestMethod]
		public void Perform_MustInitializeQuitButton()
		{
			context.Settings.Security.AllowTermination = false;
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowQuitButton = false, Times.Once);
			taskbar.VerifySet(t => t.ShowQuitButton = false, Times.Once);
			actionCenter.VerifySet(a => a.ShowQuitButton = true, Times.Never);
			taskbar.VerifySet(t => t.ShowQuitButton = true, Times.Never);

			actionCenter.Reset();
			taskbar.Reset();
			context.Settings.Security.AllowTermination = true;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowQuitButton = false, Times.Never);
			taskbar.VerifySet(t => t.ShowQuitButton = false, Times.Never);
			actionCenter.VerifySet(a => a.ShowQuitButton = true, Times.Once);
			taskbar.VerifySet(t => t.ShowQuitButton = true, Times.Once);
		}

		[TestMethod]
		public void Perform_MustInitializeNotifications()
		{
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowApplicationInfo = true;
			context.Settings.UserInterface.ActionCenter.ShowApplicationLog = true;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowApplicationInfo = true;
			context.Settings.UserInterface.Taskbar.ShowApplicationLog = true;

			sut.Perform();

			actionCenter.Verify(a => a.AddNotificationControl(It.IsAny<INotificationControl>()), Times.AtLeast(2));
			taskbar.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.AtLeast(2));
		}

		[TestMethod]
		public void Perform_MustNotInitializeNotifications()
		{
			var logControl = new Mock<INotificationControl>();

			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowApplicationLog = false;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowApplicationLog = false;

			uiFactory
				.Setup(f => f.CreateNotificationControl(It.IsAny<INotification>(), It.IsAny<Location>()))
				.Returns(logControl.Object);

			sut.Perform();

			actionCenter.Verify(a => a.AddNotificationControl(It.Is<INotificationControl>(i => i == logControl.Object)), Times.Never);
			taskbar.Verify(t => t.AddNotificationControl(It.Is<INotificationControl>(i => i == logControl.Object)), Times.Never);
		}

		[TestMethod]
		public void Perform_MustInitializeSystemComponents()
		{
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowAudio = true;
			context.Settings.UserInterface.ActionCenter.ShowKeyboardLayout = true;
			context.Settings.UserInterface.ActionCenter.ShowNetwork = true;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowAudio = true;
			context.Settings.UserInterface.Taskbar.ShowKeyboardLayout = true;
			context.Settings.UserInterface.Taskbar.ShowNetwork = true;

			systemInfo.SetupGet(s => s.HasBattery).Returns(true);
			uiFactory.Setup(f => f.CreateAudioControl(It.IsAny<IAudio>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreateKeyboardLayoutControl(It.IsAny<IKeyboard>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreatePowerSupplyControl(It.IsAny<IPowerSupply>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreateNetworkControl(It.IsAny<INetworkAdapter>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);

			sut.Perform();

			audio.Verify(a => a.Initialize(), Times.Once);
			powerSupply.Verify(p => p.Initialize(), Times.Once);
			networkAdapter.Verify(w => w.Initialize(), Times.Once);
			keyboard.Verify(k => k.Initialize(), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemControl>()), Times.Exactly(4));
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemControl>()), Times.Exactly(4));
		}

		[TestMethod]
		public void Perform_MustNotInitializeSystemComponents()
		{
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = true;
			context.Settings.UserInterface.ActionCenter.ShowAudio = false;
			context.Settings.UserInterface.ActionCenter.ShowKeyboardLayout = false;
			context.Settings.UserInterface.ActionCenter.ShowNetwork = false;
			context.Settings.UserInterface.Taskbar.EnableTaskbar = true;
			context.Settings.UserInterface.Taskbar.ShowAudio = false;
			context.Settings.UserInterface.Taskbar.ShowKeyboardLayout = false;
			context.Settings.UserInterface.Taskbar.ShowNetwork = false;

			systemInfo.SetupGet(s => s.HasBattery).Returns(false);
			uiFactory.Setup(f => f.CreateAudioControl(It.IsAny<IAudio>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreateKeyboardLayoutControl(It.IsAny<IKeyboard>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreatePowerSupplyControl(It.IsAny<IPowerSupply>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);
			uiFactory.Setup(f => f.CreateNetworkControl(It.IsAny<INetworkAdapter>(), It.IsAny<Location>())).Returns(new Mock<ISystemControl>().Object);

			sut.Perform();

			audio.Verify(a => a.Initialize(), Times.Once);
			powerSupply.Verify(p => p.Initialize(), Times.Once);
			networkAdapter.Verify(w => w.Initialize(), Times.Once);
			keyboard.Verify(k => k.Initialize(), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemControl>()), Times.Never);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemControl>()), Times.Never);
		}

		[TestMethod]
		public void Perform_MustNotInitializeActionCenterIfNotEnabled()
		{
			var actionCenterActivator = new Mock<IActionCenterActivator>();

			context.Activators.Add(actionCenterActivator.Object);
			context.Settings.UserInterface.ActionCenter.EnableActionCenter = false;

			sut.Perform();

			actionCenter.VerifyNoOtherCalls();
			actionCenterActivator.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Perform_MustNotInitializeTaskviewActivatorIfNotEnabled()
		{
			var taskviewActivator = new Mock<ITaskviewActivator>();

			context.Activators.Add(taskviewActivator.Object);
			context.Settings.Keyboard.AllowAltTab = false;

			sut.Perform();

			taskview.Verify(t => t.Register(It.IsAny<ITaskviewActivator>()), Times.Never);
			taskviewActivator.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Perform_MustNotInitializeTaskbarIfNotEnabled()
		{
			context.Settings.UserInterface.Taskbar.EnableTaskbar = false;
			sut.Perform();
			taskbar.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Perform_MustNotInitializeTerminationActivatorIfNotEnabled()
		{
			var terminationActivator = new Mock<ITerminationActivator>();

			context.Activators.Add(terminationActivator.Object);
			context.Settings.Security.AllowTermination = false;

			sut.Perform();

			terminationActivator.Verify(a => a.Start(), Times.Never);
		}

		[TestMethod]
		public void Revert_MustTerminateActivators()
		{
			var actionCenterActivator = new Mock<IActionCenterActivator>();
			var taskviewActivator = new Mock<ITaskviewActivator>();
			var terminationActivator = new Mock<ITerminationActivator>();

			context.Activators.Add(actionCenterActivator.Object);
			context.Activators.Add(taskviewActivator.Object);
			context.Activators.Add(terminationActivator.Object);

			sut.Revert();

			actionCenterActivator.Verify(a => a.Stop(), Times.Once);
			taskviewActivator.Verify(a => a.Stop(), Times.Once);
			terminationActivator.Verify(a => a.Stop(), Times.Once);
		}

		[TestMethod]
		public void Revert_MustTerminateControllers()
		{
			sut.Revert();

			aboutNotification.Verify(c => c.Terminate(), Times.Once);
			audio.Verify(a => a.Terminate(), Times.Once);
			logNotification.Verify(c => c.Terminate(), Times.Once);
			powerSupply.Verify(p => p.Terminate(), Times.Once);
			keyboard.Verify(k => k.Terminate(), Times.Once);
			networkAdapter.Verify(w => w.Terminate(), Times.Once);
		}
	}
}
