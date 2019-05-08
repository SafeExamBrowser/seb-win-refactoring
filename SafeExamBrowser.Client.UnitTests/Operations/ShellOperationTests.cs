/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ShellOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private List<IActionCenterActivator> activators;
		private ActionCenterSettings actionCenterSettings;
		private Mock<ILogger> logger;
		private TaskbarSettings taskbarSettings;
		private Mock<ITerminationActivator> terminationActivator;
		private Mock<INotificationInfo> aboutInfo;
		private Mock<INotificationController> aboutController;
		private Mock<INotificationInfo> logInfo;
		private Mock<INotificationController> logController;
		private Mock<ISystemComponent<ISystemKeyboardLayoutControl>> keyboardLayout;
		private Mock<ISystemComponent<ISystemPowerSupplyControl>> powerSupply;
		private Mock<ISystemComponent<ISystemWirelessNetworkControl>> wirelessNetwork;
		private Mock<ISystemInfo> systemInfo;
		private Mock<ITaskbar> taskbar;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ShellOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			activators = new List<IActionCenterActivator>();
			actionCenterSettings = new ActionCenterSettings();
			logger = new Mock<ILogger>();
			aboutInfo = new Mock<INotificationInfo>();
			aboutController = new Mock<INotificationController>();
			logInfo = new Mock<INotificationInfo>();
			logController = new Mock<INotificationController>();
			keyboardLayout = new Mock<ISystemComponent<ISystemKeyboardLayoutControl>>();
			powerSupply = new Mock<ISystemComponent<ISystemPowerSupplyControl>>();
			wirelessNetwork = new Mock<ISystemComponent<ISystemWirelessNetworkControl>>();
			systemInfo = new Mock<ISystemInfo>();
			taskbar = new Mock<ITaskbar>();
			taskbarSettings = new TaskbarSettings();
			terminationActivator = new Mock<ITerminationActivator>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			uiFactory.Setup(u => u.CreateNotificationControl(It.IsAny<INotificationInfo>(), It.IsAny<Location>())).Returns(new Mock<INotificationControl>().Object);

			sut = new ShellOperation(
				actionCenter.Object,
				activators,
				actionCenterSettings,
				logger.Object,
				aboutInfo.Object,
				aboutController.Object,
				logInfo.Object,
				logController.Object,
				keyboardLayout.Object,
				powerSupply.Object,
				wirelessNetwork.Object,
				systemInfo.Object,
				taskbar.Object,
				taskbarSettings,
				terminationActivator.Object,
				text.Object,
				uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeActivators()
		{
			var activatorMocks = new List<Mock<IActionCenterActivator>>
			{
				new Mock<IActionCenterActivator>(),
				new Mock<IActionCenterActivator>(),
				new Mock<IActionCenterActivator>()
			};

			actionCenterSettings.EnableActionCenter = true;
			
			foreach (var activator in activatorMocks)
			{
				activators.Add(activator.Object);
			}

			sut.Perform();

			terminationActivator.Verify(t => t.Start(), Times.Once);

			foreach (var activator in activatorMocks)
			{
				activator.Verify(a => a.Start(), Times.Once);
			}
		}

		[TestMethod]
		public void Perform_MustInitializeClock()
		{
			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowClock = true;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowClock = true;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowClock = true, Times.Once);
			taskbar.VerifySet(t => t.ShowClock = true, Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotInitializeClock()
		{
			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowClock = false;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowClock = false;

			sut.Perform();

			actionCenter.VerifySet(a => a.ShowClock = false, Times.Once);
			taskbar.VerifySet(t => t.ShowClock = false, Times.Once);
		}

		[TestMethod]
		public void Perform_MustInitializeNotifications()
		{
			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowApplicationInfo = true;
			actionCenterSettings.ShowApplicationLog = true;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowApplicationInfo = true;
			taskbarSettings.ShowApplicationLog = true;

			sut.Perform();

			actionCenter.Verify(a => a.AddNotificationControl(It.IsAny<INotificationControl>()), Times.AtLeast(2));
			taskbar.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.AtLeast(2));
		}

		[TestMethod]
		public void Perform_MustNotInitializeNotifications()
		{
			var logControl = new Mock<INotificationControl>();

			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowApplicationLog = false;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowApplicationLog = false;

			uiFactory.Setup(f => f.CreateNotificationControl(It.Is<INotificationInfo>(i => i == logInfo.Object), It.IsAny<Location>())).Returns(logControl.Object);

			sut.Perform();

			actionCenter.Verify(a => a.AddNotificationControl(It.Is<INotificationControl>(i => i == logControl.Object)), Times.Never);
			taskbar.Verify(t => t.AddNotificationControl(It.Is<INotificationControl>(i => i == logControl.Object)), Times.Never);
		}

		[TestMethod]
		public void Perform_MustInitializeSystemComponents()
		{
			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowKeyboardLayout = true;
			actionCenterSettings.ShowWirelessNetwork = true;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowKeyboardLayout = true;
			taskbarSettings.ShowWirelessNetwork = true;

			systemInfo.SetupGet(s => s.HasBattery).Returns(true);
			uiFactory.Setup(f => f.CreateKeyboardLayoutControl(It.IsAny<Location>())).Returns(new Mock<ISystemKeyboardLayoutControl>().Object);
			uiFactory.Setup(f => f.CreatePowerSupplyControl(It.IsAny<Location>())).Returns(new Mock<ISystemPowerSupplyControl>().Object);
			uiFactory.Setup(f => f.CreateWirelessNetworkControl(It.IsAny<Location>())).Returns(new Mock<ISystemWirelessNetworkControl>().Object);

			sut.Perform();

			keyboardLayout.Verify(k => k.Initialize(), Times.Once);
			powerSupply.Verify(p => p.Initialize(), Times.Once);
			wirelessNetwork.Verify(w => w.Initialize(), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemKeyboardLayoutControl>()), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemPowerSupplyControl>()), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemWirelessNetworkControl>()), Times.Once);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemKeyboardLayoutControl>()), Times.Once);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemPowerSupplyControl>()), Times.Once);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemWirelessNetworkControl>()), Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotInitializeSystemComponents()
		{
			actionCenterSettings.EnableActionCenter = true;
			actionCenterSettings.ShowKeyboardLayout = false;
			actionCenterSettings.ShowWirelessNetwork = false;
			taskbarSettings.EnableTaskbar = true;
			taskbarSettings.ShowKeyboardLayout = false;
			taskbarSettings.ShowWirelessNetwork = false;

			systemInfo.SetupGet(s => s.HasBattery).Returns(false);
			uiFactory.Setup(f => f.CreateKeyboardLayoutControl(It.IsAny<Location>())).Returns(new Mock<ISystemKeyboardLayoutControl>().Object);
			uiFactory.Setup(f => f.CreatePowerSupplyControl(It.IsAny<Location>())).Returns(new Mock<ISystemPowerSupplyControl>().Object);
			uiFactory.Setup(f => f.CreateWirelessNetworkControl(It.IsAny<Location>())).Returns(new Mock<ISystemWirelessNetworkControl>().Object);

			sut.Perform();

			keyboardLayout.Verify(k => k.Initialize(), Times.Once);
			powerSupply.Verify(p => p.Initialize(), Times.Once);
			wirelessNetwork.Verify(w => w.Initialize(), Times.Once);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemKeyboardLayoutControl>()), Times.Never);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemPowerSupplyControl>()), Times.Never);
			actionCenter.Verify(a => a.AddSystemControl(It.IsAny<ISystemWirelessNetworkControl>()), Times.Never);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemKeyboardLayoutControl>()), Times.Never);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemPowerSupplyControl>()), Times.Never);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemWirelessNetworkControl>()), Times.Never);
		}

		[TestMethod]
		public void Perform_MustNotInitializeActionCenterIfNotEnabled()
		{
			actionCenterSettings.EnableActionCenter = false;
			sut.Perform();
			actionCenter.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Perform_MustNotInitializeTaskbarIfNotEnabled()
		{
			taskbarSettings.EnableTaskbar = false;
			sut.Perform();
			taskbar.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Revert_MustTerminateActivators()
		{
			var activatorMocks = new List<Mock<IActionCenterActivator>>
			{
				new Mock<IActionCenterActivator>(),
				new Mock<IActionCenterActivator>(),
				new Mock<IActionCenterActivator>()
			};

			actionCenterSettings.EnableActionCenter = true;

			foreach (var activator in activatorMocks)
			{
				activators.Add(activator.Object);
			}

			sut.Revert();

			terminationActivator.Verify(t => t.Stop(), Times.Once);

			foreach (var activator in activatorMocks)
			{
				activator.Verify(a => a.Stop(), Times.Once);
			}
		}

		[TestMethod]
		public void Revert_MustTerminateControllers()
		{
			sut.Revert();

			aboutController.Verify(c => c.Terminate(), Times.Once);
			logController.Verify(c => c.Terminate(), Times.Once);
			keyboardLayout.Verify(k => k.Terminate(), Times.Once);
			powerSupply.Verify(p => p.Terminate(), Times.Once);
			wirelessNetwork.Verify(w => w.Terminate(), Times.Once);
		}
	}
}
