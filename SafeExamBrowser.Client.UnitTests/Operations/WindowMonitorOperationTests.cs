/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class WindowMonitorOperationTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IWindowMonitor> windowMonitorMock;

		private WindowMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			windowMonitorMock = new Mock<IWindowMonitor>();

			sut = new WindowMonitorOperation(loggerMock.Object, windowMonitorMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var order = 0;

			windowMonitorMock.Setup(w => w.HideAllWindows()).Callback(() => Assert.AreEqual(++order, 1));
			windowMonitorMock.Setup(w => w.StartMonitoringWindows()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Perform();

			windowMonitorMock.Verify(w => w.HideAllWindows(), Times.Once);
			windowMonitorMock.Verify(w => w.StartMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			var order = 0;

			windowMonitorMock.Setup(w => w.StopMonitoringWindows()).Callback(() => Assert.AreEqual(++order, 1));
			windowMonitorMock.Setup(w => w.RestoreHiddenWindows()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Revert();

			windowMonitorMock.Verify(w => w.StopMonitoringWindows(), Times.Once);
			windowMonitorMock.Verify(w => w.RestoreHiddenWindows(), Times.Once);
		}
	}
}
