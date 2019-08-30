/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class BrowserOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private Mock<IApplication> application;
		private Mock<ILogger> logger;
		private Mock<ITaskbar> taskbar;
		private Mock<IUserInterfaceFactory> uiFactory;

		private BrowserOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			application = new Mock<IApplication>();
			logger = new Mock<ILogger>();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			sut = new BrowserOperation(actionCenter.Object, application.Object, logger.Object, taskbar.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustPeformCorrectly()
		{
			sut.Perform();

			application.Verify(c => c.Initialize(), Times.Once);
			// TODO controller.Verify(c => c.RegisterApplicationControl(It.IsAny<IApplicationControl>()), Times.Exactly(2));
			actionCenter.Verify(a => a.AddApplicationControl(It.IsAny<IApplicationControl>()), Times.Once);
			taskbar.Verify(t => t.AddApplicationControl(It.IsAny<IApplicationControl>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();
			application.Verify(c => c.Terminate(), Times.Once);
		}
	}
}
