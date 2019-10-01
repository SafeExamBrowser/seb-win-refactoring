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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class DisplayMonitorOperationTests
	{
		private ClientContext context;
		private Mock<IDisplayMonitor> displayMonitorMock;
		private Mock<ILogger> loggerMock;
		private Mock<ITaskbar> taskbarMock;

		private DisplayMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			displayMonitorMock = new Mock<IDisplayMonitor>();
			loggerMock = new Mock<ILogger>();
			taskbarMock = new Mock<ITaskbar>();

			sut = new DisplayMonitorOperation(context, displayMonitorMock.Object, loggerMock.Object, taskbarMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var order = 0;

			displayMonitorMock.Setup(d => d.PreventSleepMode()).Callback(() => Assert.AreEqual(++order, 1));
			displayMonitorMock.Setup(d => d.InitializePrimaryDisplay(It.IsAny<int>())).Callback(() => Assert.AreEqual(++order, 2));
			displayMonitorMock.Setup(d => d.StartMonitoringDisplayChanges()).Callback(() => Assert.AreEqual(++order, 3));

			sut.Perform();

			displayMonitorMock.Verify(d => d.PreventSleepMode(), Times.Once);
			displayMonitorMock.Verify(d => d.InitializePrimaryDisplay(It.IsAny<int>()), Times.Once);
			displayMonitorMock.Verify(d => d.StartMonitoringDisplayChanges(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			var order = 0;

			displayMonitorMock.Setup(d => d.StopMonitoringDisplayChanges()).Callback(() => Assert.AreEqual(++order, 1));
			displayMonitorMock.Setup(d => d.ResetPrimaryDisplay()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Revert();

			displayMonitorMock.Verify(d => d.StopMonitoringDisplayChanges(), Times.Once);
			displayMonitorMock.Verify(d => d.ResetPrimaryDisplay(), Times.Once);
		}
	}
}
