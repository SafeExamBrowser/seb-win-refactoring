/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour;

namespace SafeExamBrowser.Core.UnitTests.Behaviour
{
	[TestClass]
	public class RuntimeControllerTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IProcessMonitor> processMonitorMock;
		private Mock<ITaskbar> taskbarMock;
		private Mock<IWindowMonitor> windowMonitorMock;
		private Mock<IWorkingArea> workingAreaMock;

		private IRuntimeController sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			processMonitorMock = new Mock<IProcessMonitor>();
			taskbarMock = new Mock<ITaskbar>();
			windowMonitorMock= new Mock<IWindowMonitor>();
			workingAreaMock = new Mock<IWorkingArea>();

			sut = new RuntimeController(
				loggerMock.Object,
				processMonitorMock.Object,
				taskbarMock.Object,
				windowMonitorMock.Object,
				workingAreaMock.Object);

			sut.Start();
		}

		[TestMethod]
		public void MustHandleExplorerStartCorrectly()
		{
			var order = 0;
			var processManager = 0;
			var workingArea = 0;
			var taskbar = 0;

			processMonitorMock.Setup(p => p.CloseExplorerShell()).Callback(() => processManager = ++order);
			workingAreaMock.Setup(w => w.InitializeFor(taskbarMock.Object)).Callback(() => workingArea = ++order);
			taskbarMock.Setup(t => t.InitializeBounds()).Callback(() => taskbar = ++order);

			processMonitorMock.Raise(p => p.ExplorerStarted += null);

			processMonitorMock.Verify(p => p.CloseExplorerShell(), Times.Once);
			workingAreaMock.Verify(w => w.InitializeFor(taskbarMock.Object), Times.Once);
			taskbarMock.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(processManager == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(taskbar == 3);
		}

		[TestMethod]
		public void MustHandleAllowedWindowChangeCorrectly()
		{
			var window = new IntPtr(12345);

			processMonitorMock.Setup(p => p.BelongsToAllowedProcess(window)).Returns(true);

			windowMonitorMock.Raise(w => w.WindowChanged += null, window);

			processMonitorMock.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitorMock.Verify(w => w.Hide(window), Times.Never);
			windowMonitorMock.Verify(w => w.Close(window), Times.Never);
		}

		[TestMethod]
		public void MustHandleUnallowedWindowHideCorrectly()
		{
			var order = 0;
			var belongs = 0;
			var hide = 0;
			var window = new IntPtr(12345);

			processMonitorMock.Setup(p => p.BelongsToAllowedProcess(window)).Returns(false).Callback(() => belongs = ++order);
			windowMonitorMock.Setup(w => w.Hide(window)).Returns(true).Callback(() => hide = ++order);

			windowMonitorMock.Raise(w => w.WindowChanged += null, window);

			processMonitorMock.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitorMock.Verify(w => w.Hide(window), Times.Once);
			windowMonitorMock.Verify(w => w.Close(window), Times.Never);

			Assert.IsTrue(belongs == 1);
			Assert.IsTrue(hide == 2);
		}

		[TestMethod]
		public void MustHandleUnallowedWindowCloseCorrectly()
		{
			var order = 0;
			var belongs = 0;
			var hide = 0;
			var close = 0;
			var window = new IntPtr(12345);

			processMonitorMock.Setup(p => p.BelongsToAllowedProcess(window)).Returns(false).Callback(() => belongs = ++order);
			windowMonitorMock.Setup(w => w.Hide(window)).Returns(false).Callback(() => hide = ++order);
			windowMonitorMock.Setup(w => w.Close(window)).Callback(() => close = ++order);

			windowMonitorMock.Raise(w => w.WindowChanged += null, window);

			processMonitorMock.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitorMock.Verify(w => w.Hide(window), Times.Once);
			windowMonitorMock.Verify(w => w.Close(window), Times.Once);

			Assert.IsTrue(belongs == 1);
			Assert.IsTrue(hide == 2);
			Assert.IsTrue(close == 3);
		}

		[TestCleanup]
		public void Cleanup()
		{
			sut.Stop();
		}
	}
}
