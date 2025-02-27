/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Applications.Events;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications.UnitTests
{
	[TestClass]
	public class ExternalApplicationTests
	{
		private Mock<IApplicationMonitor> applicationMonitor;
		private string executablePath;
		private Mock<IModuleLogger> logger;
		private Mock<INativeMethods> nativeMethods;
		private Mock<IProcessFactory> processFactory;
		private WhitelistApplication settings;

		private ExternalApplication sut;

		[TestInitialize]
		public void Initialize()
		{
			applicationMonitor = new Mock<IApplicationMonitor>();
			executablePath = @"C:\Some\Random\Path\Application.exe";
			logger = new Mock<IModuleLogger>();
			nativeMethods = new Mock<INativeMethods>();
			processFactory = new Mock<IProcessFactory>();
			settings = new WhitelistApplication();

			logger.Setup(l => l.CloneFor(It.IsAny<string>())).Returns(new Mock<IModuleLogger>().Object);

			sut = new ExternalApplication(applicationMonitor.Object, executablePath, logger.Object, nativeMethods.Object, processFactory.Object, settings, 1);
		}

		[TestMethod]
		public void GetWindows_MustCorrectlyReturnOpenWindows()
		{
			var openWindows = new List<IntPtr> { new IntPtr(123), new IntPtr(234), new IntPtr(456), new IntPtr(345), new IntPtr(567), new IntPtr(789) };
			var process1 = new Mock<IProcess>();
			var process2 = new Mock<IProcess>();
			var sync = new AutoResetEvent(false);

			nativeMethods.Setup(n => n.GetOpenWindows()).Returns(openWindows);
			nativeMethods.Setup(n => n.GetProcessIdFor(It.Is<IntPtr>(p => p == new IntPtr(234)))).Returns(1234);
			nativeMethods.Setup(n => n.GetProcessIdFor(It.Is<IntPtr>(p => p == new IntPtr(345)))).Returns(1234);
			nativeMethods.Setup(n => n.GetProcessIdFor(It.Is<IntPtr>(p => p == new IntPtr(567)))).Returns(5678);
			process1.Setup(p => p.TryClose(It.IsAny<int>())).Returns(false);
			process1.Setup(p => p.TryKill(It.IsAny<int>())).Returns(true);
			process1.SetupGet(p => p.Id).Returns(1234);
			process2.Setup(p => p.TryClose(It.IsAny<int>())).Returns(true);
			process2.SetupGet(p => p.Id).Returns(5678);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process1.Object);

			sut.WindowsChanged += () => sync.Set();
			sut.Initialize();
			sut.Start();

			applicationMonitor.Raise(m => m.InstanceStarted += null, sut.Id, process2.Object);

			sync.WaitOne();
			sync.WaitOne();

			var windows = sut.GetWindows();

			Assert.AreEqual(3, windows.Count());
			Assert.IsTrue(windows.Any(w => w.Handle == new IntPtr(234)));
			Assert.IsTrue(windows.Any(w => w.Handle == new IntPtr(345)));
			Assert.IsTrue(windows.Any(w => w.Handle == new IntPtr(567)));

			nativeMethods.Setup(n => n.GetOpenWindows()).Returns(openWindows.Skip(2));
			Task.Run(() => process2.Raise(p => p.Terminated += null, default(int)));

			sync.WaitOne();
			sync.WaitOne();

			windows = sut.GetWindows();

			Assert.AreEqual(1, windows.Count());
			Assert.IsTrue(windows.Any(w => w.Handle != new IntPtr(234)));
			Assert.IsTrue(windows.Any(w => w.Handle == new IntPtr(345)));
			Assert.IsTrue(windows.All(w => w.Handle != new IntPtr(567)));
		}

		[TestMethod]
		public void Initialize_MustInitializeCorrectly()
		{
			settings.AutoStart = new Random().Next(2) == 1;
			settings.Description = "Some Description";

			sut.Initialize();

			applicationMonitor.VerifyAdd(a => a.InstanceStarted += It.IsAny<InstanceStartedEventHandler>(), Times.Once);

			Assert.AreEqual(settings.AutoStart, sut.AutoStart);
			Assert.AreEqual(executablePath, (sut.Icon as EmbeddedIconResource).FilePath);
			Assert.AreEqual(settings.Id, settings.Id);
			Assert.AreEqual(settings.DisplayName, sut.Name);
			Assert.AreEqual(settings.Description ?? settings.DisplayName, sut.Tooltip);
		}

		[TestMethod]
		public void Start_MustCreateInstanceCorrectly()
		{
			settings.Arguments.Add("some_parameter");
			settings.Arguments.Add("another_parameter");
			settings.Arguments.Add("yet another parameter");

			sut.Start();

			processFactory.Verify(f => f.StartNew(executablePath, It.Is<string[]>(args => args.All(a => settings.Arguments.Contains(a)))), Times.Once);
		}

		[TestMethod]
		public void Start_MustHandleFailureGracefully()
		{
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Throws<Exception>();

			sut.Start();

			logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
			processFactory.Verify(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
		}

		[TestMethod]
		public void Start_MustRemoveInstanceCorrectlyWhenTerminated()
		{
			var eventCount = 0;
			var openWindows = new List<IntPtr> { new IntPtr(123), new IntPtr(234), new IntPtr(456), new IntPtr(345), new IntPtr(567), new IntPtr(789), };
			var process = new Mock<IProcess>();
			var sync = new AutoResetEvent(false);

			nativeMethods.Setup(n => n.GetOpenWindows()).Returns(openWindows);
			nativeMethods.Setup(n => n.GetProcessIdFor(It.Is<IntPtr>(p => p == new IntPtr(234)))).Returns(1234);
			process.Setup(p => p.TryClose(It.IsAny<int>())).Returns(false);
			process.Setup(p => p.TryKill(It.IsAny<int>())).Returns(true);
			process.SetupGet(p => p.Id).Returns(1234);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object);

			sut.WindowsChanged += () =>
			{
				eventCount++;
				sync.Set();
			};

			sut.Initialize();
			sut.Start();

			sync.WaitOne();

			Assert.AreEqual(1, sut.GetWindows().Count());

			process.Raise(p => p.Terminated += null, default(int));

			Assert.AreEqual(2, eventCount);
			Assert.AreEqual(0, sut.GetWindows().Count());
		}

		[TestMethod]
		public void Terminate_MustStopAllInstancesCorrectly()
		{
			var process1 = new Mock<IProcess>();
			var process2 = new Mock<IProcess>();

			process1.Setup(p => p.TryClose(It.IsAny<int>())).Returns(false);
			process1.Setup(p => p.TryKill(It.IsAny<int>())).Returns(true);
			process1.SetupGet(p => p.Id).Returns(1234);
			process2.Setup(p => p.TryClose(It.IsAny<int>())).Returns(true);
			process2.SetupGet(p => p.Id).Returns(5678);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process1.Object);

			sut.Initialize();
			sut.Start();

			applicationMonitor.Raise(m => m.InstanceStarted += null, sut.Id, process2.Object);
			sut.Terminate();

			process1.Verify(p => p.TryClose(It.IsAny<int>()), Times.AtLeastOnce);
			process1.Verify(p => p.TryKill(It.IsAny<int>()), Times.Once);
			process2.Verify(p => p.TryClose(It.IsAny<int>()), Times.Once);
			process2.Verify(p => p.TryKill(It.IsAny<int>()), Times.Never);
		}

		[TestMethod]
		public void Terminate_MustHandleFailureGracefully()
		{
			var process = new Mock<IProcess>();

			process.Setup(p => p.TryClose(It.IsAny<int>())).Throws<Exception>();
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object);

			sut.Initialize();
			sut.Start();
			sut.Terminate();

			process.Verify(p => p.TryClose(It.IsAny<int>()), Times.AtLeastOnce);
			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.Never);
		}
	}
}
