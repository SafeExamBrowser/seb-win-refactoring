/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Notifications;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.UnitTests.Notifications
{
	[TestClass]
	public class LogNotificationControllerTests
	{
		private Mock<ILogger> logger;
		private Mock<IUserInterfaceFactory> uiFactory;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			uiFactory = new Mock<IUserInterfaceFactory>();
		}

		[TestMethod]
		public void MustCloseWindowWhenTerminating()
		{
			var window = new Mock<IWindow>();
			var sut = new LogNotificationController(logger.Object, uiFactory.Object);

			uiFactory.Setup(u => u.CreateLogWindow(It.IsAny<ILogger>())).Returns(window.Object);

			sut.Activate();
			sut.Terminate();

			window.Verify(w => w.Close());
		}

		[TestMethod]
		public void MustOpenOnlyOneWindow()
		{
			var window = new Mock<IWindow>();
			var sut = new LogNotificationController(logger.Object, uiFactory.Object);

			uiFactory.Setup(u => u.CreateLogWindow(It.IsAny<ILogger>())).Returns(window.Object);

			sut.Activate();
			sut.Activate();
			sut.Activate();
			sut.Activate();
			sut.Activate();

			uiFactory.Verify(u => u.CreateLogWindow(It.IsAny<ILogger>()), Times.Once);
			window.Verify(u => u.Show(), Times.Once);
			window.Verify(u => u.BringToForeground(), Times.Exactly(4));
		}

		[TestMethod]
		public void MustNotFailToTerminateIfNotStarted()
		{
			var sut = new LogNotificationController(logger.Object, uiFactory.Object);

			sut.Terminate();
		}
	}
}
