/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Applications;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class BrowserOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IApplicationController> controller;
		private Mock<IApplicationInfo> appInfo;
		private Mock<ILogger> logger;
		private Mock<ITaskbar> taskbar;
		private Mock<IUserInterfaceFactory> uiFactory;

		private BrowserOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			controller = new Mock<IApplicationController>();
			appInfo = new Mock<IApplicationInfo>();
			logger = new Mock<ILogger>();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			sut = new BrowserOperation(actionCenter.Object, controller.Object, appInfo.Object, logger.Object, taskbar.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustPeformCorrectly()
		{
			var order = 0;

			controller.Setup(c => c.Initialize()).Callback(() => Assert.AreEqual(++order, 1));
			controller.Setup(c => c.RegisterApplicationControl(It.IsAny<IApplicationControl>())).Callback(() => Assert.AreEqual(++order, 2));
			taskbar.Setup(t => t.AddApplicationControl(It.IsAny<IApplicationControl>())).Callback(() => Assert.AreEqual(++order, 3));

			sut.Perform();

			controller.Verify(c => c.Initialize(), Times.Once);
			controller.Verify(c => c.RegisterApplicationControl(It.IsAny<IApplicationControl>()), Times.Once);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			controller.Verify(c => c.Terminate(), Times.Once);
		}
	}
}
