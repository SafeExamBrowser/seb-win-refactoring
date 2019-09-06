/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class KioskModeOperationTests
	{
		private SessionConfiguration currentSession;
		private ApplicationSettings currentSettings;
		private Mock<IDesktopFactory> desktopFactory;
		private Mock<IExplorerShell> explorerShell;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private ApplicationSettings nextSettings;
		private Mock<IProcessFactory> processFactory;
		private SessionContext sessionContext;

		private KioskModeOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new SessionConfiguration();
			currentSettings = new ApplicationSettings();
			desktopFactory = new Mock<IDesktopFactory>();
			explorerShell = new Mock<IExplorerShell>();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new ApplicationSettings();
			processFactory = new Mock<IProcessFactory>();
			sessionContext = new SessionContext();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;
			sessionContext.Current = currentSession;
			sessionContext.Next = nextSession;

			sut = new KioskModeOperation(desktopFactory.Object, explorerShell.Object, logger.Object, processFactory.Object, sessionContext);
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
			var suspend = 0;

			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Callback(() => getCurrrent = ++order).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Callback(() => createNew = ++order).Returns(newDesktop.Object);
			newDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.SetupSet(f => f.StartupDesktop = It.IsAny<IDesktop>()).Callback(() => setStartup = ++order);
			explorerShell.Setup(s => s.Suspend()).Callback(() => suspend = ++order);

			var result = sut.Perform();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);

			Assert.AreEqual(1, getCurrrent);
			Assert.AreEqual(2, createNew);
			Assert.AreEqual(3, activate);
			Assert.AreEqual(4, setStartup);
			Assert.AreEqual(5, suspend);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeDisableExplorerShell()
		{
			var order = 0;

			nextSettings.KioskMode = KioskMode.DisableExplorerShell;
			explorerShell.Setup(s => s.HideAllWindows()).Callback(() => Assert.AreEqual(1, ++order));
			explorerShell.Setup(s => s.Terminate()).Callback(() => Assert.AreEqual(2, ++order));

			var result = sut.Perform();

			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchToNewKioskMode()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var result = default(OperationResult);

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			nextSettings.KioskMode = KioskMode.DisableExplorerShell;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);

			currentSettings.KioskMode = nextSettings.KioskMode;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Exactly(2));
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Exactly(2));
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);
		}

		[TestMethod]
		public void Repeat_MustNotReinitializeCreateNewDesktopIfAlreadyActive()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var success = true;

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

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
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Never);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.Resume(), Times.Never);
		}

		[TestMethod]
		public void Repeat_MustNotReinitializeDisableExplorerShellIfAlreadyActive()
		{
			var success = true;

			currentSettings.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.KioskMode = KioskMode.DisableExplorerShell;

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
			var close = 0;
			var resume = 0;

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			originalDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object)).Callback(() => setStartup = ++order);
			newDesktop.Setup(d => d.Close()).Callback(() => close = ++order);
			explorerShell.Setup(s => s.Resume()).Callback(() => resume = ++order);

			var performResult = sut.Perform();
			var revertResult = sut.Revert();

			originalDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = originalDesktop.Object, Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Once);
			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);

			Assert.AreEqual(OperationResult.Success, performResult);
			Assert.AreEqual(OperationResult.Success, revertResult);
			Assert.AreEqual(1, activate);
			Assert.AreEqual(2, setStartup);
			Assert.AreEqual(3, close);
			Assert.AreEqual(4, resume);
		}

		[TestMethod]
		public void Revert_MustCorrectlyRevertDisableExplorerShell()
		{
			var order = 0;

			currentSettings.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.KioskMode = KioskMode.DisableExplorerShell;
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
			nextSettings.KioskMode = KioskMode.None;

			Assert.AreEqual(OperationResult.Success, sut.Perform());
			Assert.AreEqual(OperationResult.Success, sut.Repeat());
			Assert.AreEqual(OperationResult.Success, sut.Revert());

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
		}
	}
}
