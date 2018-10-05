/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
			var order = 0;
			var hideAll = 0;
			var startMonitoring = 0;
			var sut = new WindowMonitorOperation(KioskMode.CreateNewDesktop, loggerMock.Object, windowMonitorMock.Object);

			windowMonitorMock.Setup(w => w.HideAllWindows()).Callback(() => hideAll = ++order);
			windowMonitorMock.Setup(w => w.StartMonitoringWindows()).Callback(() => startMonitoring = ++order);

			sut.Perform();

			windowMonitorMock.Verify(w => w.HideAllWindows(), Times.Never);
			windowMonitorMock.Verify(w => w.StartMonitoringWindows(), Times.Once);

			Assert.AreEqual(0, hideAll);
			Assert.AreEqual(1, startMonitoring);
		}

		[TestMethod]
		public void MustRevertCorrectlyForCreateNewDesktop()
		{
			var order = 0;
			var stop = 0;
			var restore = 0;
			var sut = new WindowMonitorOperation(KioskMode.CreateNewDesktop, loggerMock.Object, windowMonitorMock.Object);

			windowMonitorMock.Setup(w => w.StopMonitoringWindows()).Callback(() => stop = ++order);
			windowMonitorMock.Setup(w => w.RestoreHiddenWindows()).Callback(() => restore = ++order);

			sut.Revert();

			windowMonitorMock.Verify(w => w.StopMonitoringWindows(), Times.Once);
			windowMonitorMock.Verify(w => w.RestoreHiddenWindows(), Times.Never);

			Assert.AreEqual(0, restore);
			Assert.AreEqual(1, stop);
		}

		[TestMethod]
		public void MustPerformCorrectlyForDisableExplorerShell()
		{
			var order = 0;
			var sut = new WindowMonitorOperation(KioskMode.DisableExplorerShell, loggerMock.Object, windowMonitorMock.Object);

			windowMonitorMock.Setup(w => w.HideAllWindows()).Callback(() => Assert.AreEqual(++order, 1));
			windowMonitorMock.Setup(w => w.StartMonitoringWindows()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Perform();

			windowMonitorMock.Verify(w => w.HideAllWindows(), Times.Once);
			windowMonitorMock.Verify(w => w.StartMonitoringWindows(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectlyForDisableExplorerShell()
		{
			var order = 0;
			var sut = new WindowMonitorOperation(KioskMode.DisableExplorerShell, loggerMock.Object, windowMonitorMock.Object);

			windowMonitorMock.Setup(w => w.StopMonitoringWindows()).Callback(() => Assert.AreEqual(++order, 1));
			windowMonitorMock.Setup(w => w.RestoreHiddenWindows()).Callback(() => Assert.AreEqual(++order, 2));

			sut.Revert();

			windowMonitorMock.Verify(w => w.StopMonitoringWindows(), Times.Once);
			windowMonitorMock.Verify(w => w.RestoreHiddenWindows(), Times.Once);
		}

		[TestMethod]
		public void MustDoNothingWithoutKioskMode()
		{
			var sut = new WindowMonitorOperation(KioskMode.None, loggerMock.Object, windowMonitorMock.Object);

			windowMonitorMock.VerifyNoOtherCalls();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MustNotAllowRepeating()
		{
			var sut = new WindowMonitorOperation(KioskMode.None, loggerMock.Object, windowMonitorMock.Object);

			sut.Repeat();
		}
	}
}
