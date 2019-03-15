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

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ShellOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IEnumerable<IActionCenterActivator>> activators;
		private ActionCenterSettings actionCenterSettings;
		private Mock<ILogger> logger;
		private TaskbarSettings taskbarSettings;
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
			activators = new Mock<IEnumerable<IActionCenterActivator>>();
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
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			taskbarSettings.ShowApplicationLog = true;
			taskbarSettings.ShowKeyboardLayout = true;
			taskbarSettings.ShowWirelessNetwork = true;
			taskbarSettings.EnableTaskbar = true;
			systemInfo.SetupGet(s => s.HasBattery).Returns(true);
			uiFactory.Setup(u => u.CreateNotificationControl(It.IsAny<INotificationInfo>(), It.IsAny<Location>())).Returns(new Mock<INotificationControl>().Object);

			sut = new ShellOperation(
				actionCenter.Object,
				activators.Object,
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
				text.Object,
				uiFactory.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			keyboardLayout.Verify(k => k.Initialize(), Times.Once);
			powerSupply.Verify(p => p.Initialize(), Times.Once);
			wirelessNetwork.Verify(w => w.Initialize(), Times.Once);
			taskbar.Verify(t => t.AddSystemControl(It.IsAny<ISystemControl>()), Times.Exactly(3));
			taskbar.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Exactly(2));
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			aboutController.Verify(c => c.Terminate(), Times.Once);
			keyboardLayout.Verify(k => k.Terminate(), Times.Once);
			powerSupply.Verify(p => p.Terminate(), Times.Once);
			wirelessNetwork.Verify(w => w.Terminate(), Times.Once);
		}
	}
}
