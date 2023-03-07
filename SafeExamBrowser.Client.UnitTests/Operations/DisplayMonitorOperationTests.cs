/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class DisplayMonitorOperationTests
	{
		private ClientContext context;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<ILogger> logger;
		private Mock<ITaskbar> taskbar;

		private DisplayMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			displayMonitor = new Mock<IDisplayMonitor>();
			logger = new Mock<ILogger>();
			taskbar = new Mock<ITaskbar>();

			sut = new DisplayMonitorOperation(context, displayMonitor.Object, logger.Object, taskbar.Object);
		}

		[TestMethod]
		public void Perform_MustExecuteInCorrectOrder()
		{
			var order = 0;

			context.Settings = new AppSettings();
			context.Settings.Taskbar.EnableTaskbar = true;

			displayMonitor.Setup(d => d.PreventSleepMode()).Callback(() => Assert.AreEqual(++order, 1));
			displayMonitor.Setup(d => d.InitializePrimaryDisplay(It.IsAny<int>())).Callback(() => Assert.AreEqual(++order, 2));
			displayMonitor.Setup(d => d.StartMonitoringDisplayChanges()).Callback(() => Assert.AreEqual(++order, 3));

			sut.Perform();

			displayMonitor.Verify(d => d.PreventSleepMode(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.IsAny<int>()), Times.Once);
			displayMonitor.Verify(d => d.StartMonitoringDisplayChanges(), Times.Once);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeDisplayWithTaskbar()
		{
			int height = 25;

			context.Settings = new AppSettings();
			context.Settings.Taskbar.EnableTaskbar = true;
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);

			sut.Perform();

			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Never);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeDisplayWithoutTaskbar()
		{
			int height = 25;

			context.Settings = new AppSettings();
			context.Settings.Taskbar.EnableTaskbar = false;
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);

			sut.Perform();

			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Never);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Once);
		}

		[TestMethod]
		public void Revert_MustExecuteInCorrectOrder()
		{
			var order = 0;

			displayMonitor.Setup(d => d.StopMonitoringDisplayChanges()).Callback(() => Assert.AreEqual(++order, 1));
			displayMonitor.Setup(d => d.ResetPrimaryDisplay()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Revert();

			displayMonitor.Verify(d => d.StopMonitoringDisplayChanges(), Times.Once);
			displayMonitor.Verify(d => d.ResetPrimaryDisplay(), Times.Once);
		}
	}
}
