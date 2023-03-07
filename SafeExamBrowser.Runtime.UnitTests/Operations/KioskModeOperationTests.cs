/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class KioskModeOperationTests
	{
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private Mock<IDesktopFactory> desktopFactory;
		private Mock<IDesktopMonitor> desktopMonitor;
		private Mock<IExplorerShell> explorerShell;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private Mock<IProcessFactory> processFactory;
		private SessionContext sessionContext;

		private KioskModeOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			desktopFactory = new Mock<IDesktopFactory>();
			desktopMonitor = new Mock<IDesktopMonitor>();
			explorerShell = new Mock<IExplorerShell>();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			processFactory = new Mock<IProcessFactory>();
			sessionContext = new SessionContext();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;
			sessionContext.Current = currentSession;
			sessionContext.Next = nextSession;

			sut = new KioskModeOperation(desktopFactory.Object, desktopMonitor.Object, explorerShell.Object, logger.Object, processFactory.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeCreateNewDesktop()
		{
			var originalDesktop = new Mock<IDesktop>();
			var newDesktop = new Mock<IDesktop>();
			var order = 0;
			var getCurrrent = 0;
			var createNew = 0;
			var activate = 0;
			var setStartup = 0;
			var startMonitor = 0;

			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Callback(() => getCurrrent = ++order).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Callback(() => createNew = ++order).Returns(newDesktop.Object);
			newDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.SetupSet(f => f.StartupDesktop = It.IsAny<IDesktop>()).Callback(() => setStartup = ++order);
			desktopMonitor.Setup(m => m.Start(It.IsAny<IDesktop>())).Callback(() => startMonitor = ++order);

			var result = sut.Perform();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);

			Assert.AreEqual(OperationResult.Success, result);

			Assert.AreEqual(1, getCurrrent);
			Assert.AreEqual(2, createNew);
			Assert.AreEqual(3, activate);
			Assert.AreEqual(4, setStartup);
			Assert.AreEqual(5, startMonitor);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeDisableExplorerShell()
		{
			var order = 0;

			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			explorerShell.Setup(s => s.HideAllWindows()).Callback(() => Assert.AreEqual(1, ++order));
			explorerShell.Setup(s => s.Terminate()).Callback(() => Assert.AreEqual(2, ++order));

			var result = sut.Perform();

			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromCreateNewDesktopToDisableExplorerShell()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var close = 0;
			var hide = 0;
			var startupDesktop = 0;
			var terminate = 0;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			sut.Perform();

			desktopFactory.Reset();
			explorerShell.Reset();
			explorerShell.Setup(s => s.HideAllWindows()).Callback(() => hide = ++order);
			explorerShell.Setup(s => s.Terminate()).Callback(() => terminate = ++order);
			newDesktop.Reset();
			newDesktop.Setup(d => d.Close()).Callback(() => close = ++order);
			originalDesktop.Reset();
			originalDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.Reset();
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object)).Callback(() => startupDesktop = ++order);
			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			var result = sut.Repeat();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, activate);
			Assert.AreEqual(2, startupDesktop);
			Assert.AreEqual(3, close);
			Assert.AreEqual(4, hide);
			Assert.AreEqual(5, terminate);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromCreateNewDesktopToNone()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var close = 0;
			var startupDesktop = 0;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			sut.Perform();

			desktopFactory.Reset();
			explorerShell.Reset();
			newDesktop.Reset();
			newDesktop.Setup(d => d.Close()).Callback(() => close = ++order);
			originalDesktop.Reset();
			originalDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.Reset();
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object)).Callback(() => startupDesktop = ++order);
			nextSettings.Security.KioskMode = KioskMode.None;

			var result = sut.Repeat();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, activate);
			Assert.AreEqual(2, startupDesktop);
			Assert.AreEqual(3, close);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromDisableExplorerShellToCreateNewDesktop()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var current = 0;
			var restore = 0;
			var start = 0;
			var startupDesktop = 0;

			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			sut.Perform();

			desktopFactory.Reset();
			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object).Callback(() => current = ++order);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			explorerShell.Reset();
			explorerShell.Setup(s => s.RestoreAllWindows()).Callback(() => restore = ++order);
			explorerShell.Setup(s => s.Start()).Callback(() => start = ++order);
			newDesktop.Reset();
			newDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			originalDesktop.Reset();
			processFactory.Reset();
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == newDesktop.Object)).Callback(() => startupDesktop = ++order);
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			var result = sut.Repeat();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Activate(), Times.Once);
			originalDesktop.VerifyNoOtherCalls();
			processFactory.VerifySet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == newDesktop.Object), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, start);
			Assert.AreEqual(2, restore);
			Assert.AreEqual(3, current);
			Assert.AreEqual(4, activate);
			Assert.AreEqual(5, startupDesktop);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromDisableExplorerShellToNone()
		{
			var order = 0;
			var restore = 0;
			var start = 0;

			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			sut.Perform();

			explorerShell.Reset();
			explorerShell.Setup(s => s.RestoreAllWindows()).Callback(() => restore = ++order);
			explorerShell.Setup(s => s.Start()).Callback(() => start = ++order);
			processFactory.Reset();
			nextSettings.Security.KioskMode = KioskMode.None;

			var result = sut.Repeat();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.VerifyNoOtherCalls();
			processFactory.VerifySet(f => f.StartupDesktop = It.IsAny<IDesktop>(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, start);
			Assert.AreEqual(2, restore);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromNoneToCreateNewDesktop()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var current = 0;
			var startup = 0;

			nextSettings.Security.KioskMode = KioskMode.None;

			sut.Perform();

			desktopFactory.Reset();
			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object).Callback(() => current = ++order);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			explorerShell.Reset();
			newDesktop.Reset();
			newDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			originalDesktop.Reset();
			processFactory.Reset();
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == newDesktop.Object)).Callback(() => startup = ++order);
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			var result = sut.Repeat();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Activate(), Times.Once);
			originalDesktop.VerifyNoOtherCalls();
			processFactory.VerifySet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == newDesktop.Object), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, current);
			Assert.AreEqual(2, activate);
			Assert.AreEqual(3, startup);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchFromNoneToDisableExplorerShell()
		{
			var order = 0;
			var hide = 0;
			var terminate = 0;

			nextSettings.Security.KioskMode = KioskMode.None;

			sut.Perform();

			desktopFactory.Reset();
			explorerShell.Reset();
			explorerShell.Setup(s => s.HideAllWindows()).Callback(() => hide = ++order);
			explorerShell.Setup(s => s.Terminate()).Callback(() => terminate = ++order);
			processFactory.Reset();
			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			var result = sut.Repeat();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = It.IsAny<IDesktop>(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, hide);
			Assert.AreEqual(2, terminate);
		}

		[TestMethod]
		public void Repeat_MustNotReinitializeCreateNewDesktopIfAlreadyActive()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var success = true;

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);

			success &= sut.Perform() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;

			Assert.IsTrue(success);

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			desktopMonitor.Verify(m => m.Start(It.IsAny<IDesktop>()), Times.Once);
			desktopMonitor.Verify(m => m.Stop(), Times.Never);
			explorerShell.VerifyNoOtherCalls();
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Never);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);
		}

		[TestMethod]
		public void Repeat_MustNotReinitializeDisableExplorerShellIfAlreadyActive()
		{
			var success = true;

			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			success &= sut.Perform() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;
			success &= sut.Repeat() == OperationResult.Success;

			Assert.IsTrue(success);

			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
		}

		[TestMethod]
		public void Revert_MustCorrectlyRevertCreateNewDesktop()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var setStartup = 0;
			var stopMonitor = 0;
			var close = 0;

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			nextSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);

			var performResult = sut.Perform();

			Assert.AreEqual(OperationResult.Success, performResult);

			desktopFactory.Reset();
			desktopMonitor.Setup(m => m.Stop()).Callback(() => stopMonitor = ++order);
			explorerShell.Reset();
			originalDesktop.Reset();
			originalDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object)).Callback(() => setStartup = ++order);
			newDesktop.Reset();
			newDesktop.Setup(d => d.Close()).Callback(() => close = ++order);

			var revertResult = sut.Revert();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			originalDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = originalDesktop.Object, Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Once);

			Assert.AreEqual(OperationResult.Success, performResult);
			Assert.AreEqual(OperationResult.Success, revertResult);
			Assert.AreEqual(1, stopMonitor);
			Assert.AreEqual(2, activate);
			Assert.AreEqual(3, setStartup);
			Assert.AreEqual(4, close);
		}

		[TestMethod]
		public void Revert_MustCorrectlyRevertDisableExplorerShell()
		{
			var order = 0;

			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			explorerShell.Setup(s => s.Start()).Callback(() => Assert.AreEqual(1, ++order));
			explorerShell.Setup(s => s.RestoreAllWindows()).Callback(() => Assert.AreEqual(2, ++order));

			var performResult = sut.Perform();
			var revertResult = sut.Revert();

			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);

			Assert.AreEqual(OperationResult.Success, performResult);
			Assert.AreEqual(OperationResult.Success, revertResult);
		}

		[TestMethod]
		public void MustDoNothingWithoutKioskMode()
		{
			nextSettings.Security.KioskMode = KioskMode.None;

			Assert.AreEqual(OperationResult.Success, sut.Perform());
			Assert.AreEqual(OperationResult.Success, sut.Repeat());
			Assert.AreEqual(OperationResult.Success, sut.Repeat());
			Assert.AreEqual(OperationResult.Success, sut.Repeat());
			Assert.AreEqual(OperationResult.Success, sut.Repeat());
			Assert.AreEqual(OperationResult.Success, sut.Revert());

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
		}
	}
}
