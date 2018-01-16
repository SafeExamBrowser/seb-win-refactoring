/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Core.Behaviour.Operations;

namespace SafeExamBrowser.Core.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class TaskbarOperationTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<ITaskbarSettings> settingsMock;
		private Mock<ISplashScreen> splashScreenMock;
		private Mock<ISystemComponent<ISystemKeyboardLayoutControl>> keyboardLayoutMock;
		private Mock<ISystemComponent<ISystemPowerSupplyControl>> powerSupplyMock;
		private Mock<ISystemComponent<ISystemWirelessNetworkControl>> wirelessNetworkMock;
		private Mock<ISystemInfo> systemInfoMock;
		private Mock<ITaskbar> taskbarMock;
		private Mock<IText> textMock;
		private Mock<IUserInterfaceFactory> uiFactoryMock;

		private TaskbarOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			settingsMock = new Mock<ITaskbarSettings>();
			splashScreenMock = new Mock<ISplashScreen>();
			keyboardLayoutMock = new Mock<ISystemComponent<ISystemKeyboardLayoutControl>>();
			powerSupplyMock = new Mock<ISystemComponent<ISystemPowerSupplyControl>>();
			wirelessNetworkMock = new Mock<ISystemComponent<ISystemWirelessNetworkControl>>();
			systemInfoMock = new Mock<ISystemInfo>();
			taskbarMock = new Mock<ITaskbar>();
			textMock = new Mock<IText>();
			uiFactoryMock = new Mock<IUserInterfaceFactory>();

			settingsMock.SetupGet(s => s.AllowApplicationLog).Returns(true);
			settingsMock.SetupGet(s => s.AllowKeyboardLayout).Returns(true);
			settingsMock.SetupGet(s => s.AllowWirelessNetwork).Returns(true);
			systemInfoMock.SetupGet(s => s.HasBattery).Returns(true);
			uiFactoryMock.Setup(u => u.CreateNotification(It.IsAny<INotificationInfo>())).Returns(new Mock<INotificationButton>().Object);

			sut = new TaskbarOperation(
				loggerMock.Object,
				settingsMock.Object,
				keyboardLayoutMock.Object,
				powerSupplyMock.Object,
				wirelessNetworkMock.Object,
				systemInfoMock.Object,
				taskbarMock.Object,
				textMock.Object,
				uiFactoryMock.Object)
			{
				SplashScreen = splashScreenMock.Object
			};
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			keyboardLayoutMock.Verify(k => k.Initialize(It.IsAny<ISystemKeyboardLayoutControl>()), Times.Once);
			powerSupplyMock.Verify(p => p.Initialize(It.IsAny<ISystemPowerSupplyControl>()), Times.Once);
			wirelessNetworkMock.Verify(w => w.Initialize(It.IsAny<ISystemWirelessNetworkControl>()), Times.Once);
			taskbarMock.Verify(t => t.AddSystemControl(It.IsAny<ISystemControl>()), Times.Exactly(3));
			taskbarMock.Verify(t => t.AddNotification(It.IsAny<INotificationButton>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			keyboardLayoutMock.Verify(k => k.Terminate(), Times.Once);
			powerSupplyMock.Verify(p => p.Terminate(), Times.Once);
			wirelessNetworkMock.Verify(w => w.Terminate(), Times.Once);
		}
	}
}
