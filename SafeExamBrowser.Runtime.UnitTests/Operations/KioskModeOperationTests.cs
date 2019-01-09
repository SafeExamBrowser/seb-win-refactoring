/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class KioskModeOperationTests
	{
		private Mock<ISessionConfiguration> currentSession;
		private Settings currentSettings;
		private Mock<IDesktopFactory> desktopFactory;
		private Mock<IExplorerShell> explorerShell;
		private Mock<ILogger> logger;
		private Mock<ISessionConfiguration> nextSession;
		private Settings nextSettings;
		private Mock<IProcessFactory> processFactory;
		private SessionContext sessionContext;

		private KioskModeOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new Mock<ISessionConfiguration>();
			currentSettings = new Settings();
			desktopFactory = new Mock<IDesktopFactory>();
			explorerShell = new Mock<IExplorerShell>();
			logger = new Mock<ILogger>();
			nextSession = new Mock<ISessionConfiguration>();
			nextSettings = new Settings();
			processFactory = new Mock<IProcessFactory>();
			sessionContext = new SessionContext();

			currentSession.SetupGet(s => s.Settings).Returns(currentSettings);
			nextSession.SetupGet(s => s.Settings).Returns(nextSettings);
			sessionContext.Current = currentSession.Object;
			sessionContext.Next = nextSession.Object;

			sut = new KioskModeOperation(desktopFactory.Object, explorerShell.Object, logger.Object, processFactory.Object, sessionContext);
		}

		[TestMethod]
		public void MustCorrectlyInitializeCreateNewDesktop()
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

			sut.Perform();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);

			Assert.AreSame(sessionContext.NewDesktop, newDesktop.Object);
			Assert.AreSame(sessionContext.OriginalDesktop, originalDesktop.Object);
			Assert.AreEqual(1, getCurrrent);
			Assert.AreEqual(2, createNew);
			Assert.AreEqual(3, activate);
			Assert.AreEqual(4, setStartup);
			Assert.AreEqual(5, suspend);
		}

		[TestMethod]
		public void MustCorrectlyInitializeDisableExplorerShell()
		{
			var order = 0;

			nextSettings.KioskMode = KioskMode.DisableExplorerShell;
			explorerShell.Setup(s => s.HideAllWindows()).Callback(() => Assert.AreEqual(1, ++order));
			explorerShell.Setup(s => s.Terminate()).Callback(() => Assert.AreEqual(2, ++order));

			sut.Perform();

			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
		}

		[TestMethod]
		public void MustCorrectlyRevertCreateNewDesktop()
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
		public void MustCorrectlyRevertDisableExplorerShell()
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
		public void MustCorrectlyStartNewKioskModeWhenRepeating()
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
			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			currentSettings.KioskMode = nextSettings.KioskMode;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Exactly(2));
			explorerShell.Verify(s => s.HideAllWindows(), Times.Once);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Exactly(2));
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			Assert.AreSame(sessionContext.NewDesktop, newDesktop.Object);
			Assert.AreSame(sessionContext.OriginalDesktop, originalDesktop.Object);
		}

		[TestMethod]
		public void MustDoNothingWithoutKioskMode()
		{
			nextSettings.KioskMode = KioskMode.None;

			sut.Perform();
			sut.Repeat();
			sut.Revert();

			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustNotReinitializeCreateNewDesktopWhenRepeating()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);

			sut.Perform();
			sut.Repeat();
			sut.Repeat();
			sut.Repeat();
			sut.Repeat();
			sut.Revert();

			desktopFactory.Verify(f => f.GetCurrent(), Times.Once);
			desktopFactory.Verify(f => f.CreateNew(It.IsAny<string>()), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = newDesktop.Object, Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);

			Assert.AreSame(sessionContext.NewDesktop, newDesktop.Object);
			Assert.AreSame(sessionContext.OriginalDesktop, originalDesktop.Object);
		}

		[TestMethod]
		public void MustNotReinitializeDisableExplorerShellWhenRepeating()
		{
			currentSettings.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.KioskMode = KioskMode.DisableExplorerShell;

			sut.Perform();
			sut.Repeat();
			sut.Repeat();
			sut.Repeat();
			sut.Repeat();
			sut.Revert();

			explorerShell.Verify(s => s.Terminate(), Times.Once);
		}
	}
}
