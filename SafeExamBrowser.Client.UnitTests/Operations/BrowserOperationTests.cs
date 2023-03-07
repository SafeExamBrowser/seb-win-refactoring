/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
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
		private Mock<ITaskview> taskview;
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
			taskview = new Mock<ITaskview>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Browser = browser.Object;
			context.Settings = settings;

			sut = new BrowserOperation(actionCenter.Object, context, logger.Object, taskbar.Object, taskview.Object, uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeBrowserAndTaskview()
		{
			settings.Browser.EnableBrowser = true;

			sut.Perform();

			browser.Verify(c => c.Initialize(), Times.Once);
			taskview.Verify(t => t.Add(It.Is<IApplication>(a => a == context.Browser)), Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotInitializeBrowserIfNotEnabled()
		{
			settings.ActionCenter.EnableActionCenter = true;
			settings.Browser.EnableBrowser = false;
			settings.Taskbar.EnableTaskbar = true;

			sut.Perform();

			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Never);
			browser.Verify(c => c.Initialize(), Times.Never);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Never);
			taskview.Verify(t => t.Add(It.Is<IApplication>(a => a == context.Browser)), Times.Never);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeControls()
		{
			settings.ActionCenter.EnableActionCenter = false;
			settings.Browser.EnableBrowser = true;
			settings.Taskbar.EnableTaskbar = false;

			sut.Perform();

			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Never);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Never);

			settings.ActionCenter.EnableActionCenter = true;
			settings.Taskbar.EnableTaskbar = true;

			sut.Perform();

			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Once);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>(), true), Times.Once);
		}

		[TestMethod]
		public void Revert_MustTerminateBrowser()
		{
			settings.Browser.EnableBrowser = true;
			sut.Revert();
			browser.Verify(c => c.Terminate(), Times.Once);
		}

		[TestMethod]
		public void Revert_MustNotTerminateBrowserIfNotEnabled()
		{
			settings.Browser.EnableBrowser = false;
			sut.Revert();
			browser.Verify(c => c.Terminate(), Times.Never);
		}
	}
}
