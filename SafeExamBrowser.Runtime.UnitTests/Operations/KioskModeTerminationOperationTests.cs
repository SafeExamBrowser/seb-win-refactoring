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
	public class KioskModeTerminationOperationTests
	{
		private Mock<IDesktopFactory> desktopFactory;
		private Mock<IExplorerShell> explorerShell;
		private Mock<ILogger> logger;
		private Mock<ISessionConfiguration> nextSession;
		private Settings nextSettings;
		private Mock<IProcessFactory> processFactory;
		private SessionContext sessionContext;

		private KioskModeTerminationOperation sut;
		private Mock<ISessionConfiguration> currentSession;
		private Settings currentSettings;

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

			sut = new KioskModeTerminationOperation(desktopFactory.Object, explorerShell.Object, logger.Object, processFactory.Object, sessionContext);
		}

		[TestMethod]
		public void MustDoNothingOnPerform()
		{
			var result = sut.Perform();

			currentSession.VerifyNoOtherCalls();
			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();
			nextSession.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustCorrectlyTerminateOldKioskModeWhenRepeating()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var result = default(OperationResult);

			sessionContext.NewDesktop = newDesktop.Object;
			sessionContext.OriginalDesktop = originalDesktop.Object;
			sessionContext.ActiveMode = KioskMode.DisableExplorerShell;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Never);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			sessionContext.ActiveMode = nextSettings.KioskMode;
			nextSettings.KioskMode = KioskMode.DisableExplorerShell;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Never);
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);

			sessionContext.ActiveMode = nextSettings.KioskMode;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Exactly(2));
			explorerShell.Verify(s => s.Suspend(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Exactly(2));
			newDesktop.Verify(d => d.Activate(), Times.Never);
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);
		}

		[TestMethod]
		public void MustNotTerminateKioskModeIfSameInNextSesssion()
		{
			var newDesktop = new Mock<IDesktop>();
			var originalDesktop = new Mock<IDesktop>();
			var result = default(OperationResult);

			sessionContext.NewDesktop = newDesktop.Object;
			sessionContext.OriginalDesktop = originalDesktop.Object;
			sessionContext.ActiveMode = KioskMode.DisableExplorerShell;
			nextSettings.KioskMode = KioskMode.DisableExplorerShell;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Never);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			sessionContext.ActiveMode = KioskMode.CreateNewDesktop;
			nextSettings.KioskMode = KioskMode.CreateNewDesktop;

			result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Never);
			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.HideAllWindows(), Times.Never);
			explorerShell.Verify(s => s.RestoreAllWindows(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Never);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			var result = sut.Revert();

			currentSession.VerifyNoOtherCalls();
			desktopFactory.VerifyNoOtherCalls();
			explorerShell.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();
			nextSession.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
