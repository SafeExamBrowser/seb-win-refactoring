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
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class WindowMonitorOperationTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IWindowMonitor> windowMonitorMock;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			windowMonitorMock = new Mock<IWindowMonitor>();
		}

		[TestMethod]
		public void MustPerformCorrectlyForCreateNewDesktop()
		{
			var sut = new WindowMonitorOperation(KioskMode.CreateNewDesktop, loggerMock.Object, windowMonitorMock.Object);

			sut.Perform();

			windowMonitorMock.Verify(w => w.StartMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectlyForCreateNewDesktop()
		{
			var sut = new WindowMonitorOperation(KioskMode.CreateNewDesktop, loggerMock.Object, windowMonitorMock.Object);

			sut.Revert();

			windowMonitorMock.Verify(w => w.StopMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustPerformCorrectlyForDisableExplorerShell()
		{
			var sut = new WindowMonitorOperation(KioskMode.DisableExplorerShell, loggerMock.Object, windowMonitorMock.Object);

			sut.Perform();

			windowMonitorMock.Verify(w => w.StartMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectlyForDisableExplorerShell()
		{
			var sut = new WindowMonitorOperation(KioskMode.DisableExplorerShell, loggerMock.Object, windowMonitorMock.Object);

			sut.Revert();

			windowMonitorMock.Verify(w => w.StopMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustDoNothingWithoutKioskMode()
		{
			var sut = new WindowMonitorOperation(KioskMode.None, loggerMock.Object, windowMonitorMock.Object);

			sut.Perform();
			sut.Revert();

			windowMonitorMock.VerifyNoOtherCalls();
		}
	}
}
