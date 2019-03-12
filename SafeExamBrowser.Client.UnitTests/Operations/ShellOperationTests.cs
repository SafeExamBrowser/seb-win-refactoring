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
		private Mock<ILogger> loggerMock;
		private TaskbarSettings taskbarSettings;
		private Mock<INotificationInfo> aboutInfoMock;
		private Mock<INotificationController> aboutControllerMock;
		private Mock<INotificationInfo> logInfoMock;
		private Mock<INotificationController> logControllerMock;
		private Mock<ISystemComponent<ISystemKeyboardLayoutControl>> keyboardLayoutMock;
		private Mock<ISystemComponent<ISystemPowerSupplyControl>> powerSupplyMock;
		private Mock<ISystemComponent<ISystemWirelessNetworkControl>> wirelessNetworkMock;
		private Mock<ISystemInfo> systemInfoMock;
		private Mock<ITaskbar> taskbarMock;
		private Mock<IUserInterfaceFactory> uiFactoryMock;

		private ShellOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			activators = new Mock<IEnumerable<IActionCenterActivator>>();
			actionCenterSettings = new ActionCenterSettings();
			loggerMock = new Mock<ILogger>();
			aboutInfoMock = new Mock<INotificationInfo>();
			aboutControllerMock = new Mock<INotificationController>();
			logInfoMock = new Mock<INotificationInfo>();
			logControllerMock = new Mock<INotificationController>();
			keyboardLayoutMock = new Mock<ISystemComponent<ISystemKeyboardLayoutControl>>();
			powerSupplyMock = new Mock<ISystemComponent<ISystemPowerSupplyControl>>();
			wirelessNetworkMock = new Mock<ISystemComponent<ISystemWirelessNetworkControl>>();
			systemInfoMock = new Mock<ISystemInfo>();
			taskbarMock = new Mock<ITaskbar>();
			taskbarSettings = new TaskbarSettings();
			uiFactoryMock = new Mock<IUserInterfaceFactory>();

			taskbarSettings.ShowApplicationLog = true;
			taskbarSettings.ShowKeyboardLayout = true;
			taskbarSettings.AllowWirelessNetwork = true;
			taskbarSettings.EnableTaskbar = true;
			systemInfoMock.SetupGet(s => s.HasBattery).Returns(true);
			uiFactoryMock.Setup(u => u.CreateNotificationControl(It.IsAny<INotificationInfo>(), It.IsAny<Location>())).Returns(new Mock<INotificationControl>().Object);

			sut = new ShellOperation(
				actionCenter.Object,
				activators.Object,
				actionCenterSettings,
				loggerMock.Object,
				aboutInfoMock.Object,
				aboutControllerMock.Object,
				logInfoMock.Object,
				logControllerMock.Object,
				keyboardLayoutMock.Object,
				powerSupplyMock.Object,
				wirelessNetworkMock.Object,
				systemInfoMock.Object,
				taskbarMock.Object,
				taskbarSettings,
				uiFactoryMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			keyboardLayoutMock.Verify(k => k.Initialize(), Times.Once);
			powerSupplyMock.Verify(p => p.Initialize(), Times.Once);
			wirelessNetworkMock.Verify(w => w.Initialize(), Times.Once);
			taskbarMock.Verify(t => t.AddSystemControl(It.IsAny<ISystemControl>()), Times.Exactly(3));
			taskbarMock.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Exactly(2));
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			aboutControllerMock.Verify(c => c.Terminate(), Times.Once);
			keyboardLayoutMock.Verify(k => k.Terminate(), Times.Once);
			powerSupplyMock.Verify(p => p.Terminate(), Times.Once);
			wirelessNetworkMock.Verify(w => w.Terminate(), Times.Once);
		}
	}
}
