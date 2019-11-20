/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class BrowserOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IBrowserApplication> browser;
		private ClientContext context;
		private Mock<ILogger> logger;
		private AppSettings settings;
		private Mock<ITaskbar> taskbar;
		private Mock<ITaskView> taskView;
		private Mock<IUserInterfaceFactory> uiFactory;

		private BrowserOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			browser = new Mock<IBrowserApplication>();
			context = new ClientContext();
			logger = new Mock<ILogger>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			taskView = new Mock<ITaskView>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Browser = browser.Object;
			context.Settings = settings;

			sut = new BrowserOperation(actionCenter.Object, context, logger.Object, taskbar.Object, taskView.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustPeformCorrectly()
		{
			settings.ActionCenter.EnableActionCenter = true;
			settings.Taskbar.EnableTaskbar = true;

			sut.Perform();

			browser.Verify(c => c.Initialize(), Times.Once);
			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Once);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();
			browser.Verify(c => c.Terminate(), Times.Once);
		}

		[TestMethod]
		public void TODO()
		{
			// TODO: Test initialization of task view!
			Assert.Fail("TODO");
		}
	}
}
