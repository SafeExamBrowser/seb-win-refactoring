/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class KioskModeOperationTests
	{
		private Mock<IConfigurationRepository> configuration;
		private Mock<IDesktopFactory> desktopFactory;
		private Mock<IExplorerShell> explorerShell;
		private Mock<ILogger> logger;
		private Mock<IProcessFactory> processFactory;
		private Settings settings;
		private KioskModeOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			configuration = new Mock<IConfigurationRepository>();
			desktopFactory = new Mock<IDesktopFactory>();
			explorerShell = new Mock<IExplorerShell>();
			logger = new Mock<ILogger>();
			processFactory = new Mock<IProcessFactory>();
			settings = new Settings();

			configuration.SetupGet(c => c.CurrentSettings).Returns(settings);

			sut = new KioskModeOperation(configuration.Object, desktopFactory.Object, explorerShell.Object, logger.Object, processFactory.Object);
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

			settings.KioskMode = KioskMode.CreateNewDesktop;

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

			Assert.AreEqual(1, getCurrrent);
			Assert.AreEqual(2, createNew);
			Assert.AreEqual(3, activate);
			Assert.AreEqual(4, setStartup);
			Assert.AreEqual(5, suspend);
		}

		[TestMethod]
		public void MustCorrectlyInitializeDisableExplorerShell()
		{
			settings.KioskMode = KioskMode.DisableExplorerShell;

			sut.Perform();

			explorerShell.Verify(s => s.Terminate(), Times.Once);
		}

		[TestMethod]
		public void MustCorrectlyRevertCreateNewDesktop()
		{
			var originalDesktop = new Mock<IDesktop>();
			var newDesktop = new Mock<IDesktop>();
			var order = 0;
			var activate = 0;
			var setStartup = 0;
			var close = 0;
			var resume = 0;

			settings.KioskMode = KioskMode.CreateNewDesktop;

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);
			originalDesktop.Setup(d => d.Activate()).Callback(() => activate = ++order);
			processFactory.SetupSet(f => f.StartupDesktop = It.Is<IDesktop>(d => d == originalDesktop.Object)).Callback(() => setStartup = ++order);
			newDesktop.Setup(d => d.Close()).Callback(() => close = ++order);
			explorerShell.Setup(s => s.Resume()).Callback(() => resume = ++order);

			sut.Perform();
			sut.Revert();

			originalDesktop.Verify(d => d.Activate(), Times.Once);
			processFactory.VerifySet(f => f.StartupDesktop = originalDesktop.Object, Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Once);
			explorerShell.Verify(s => s.Resume(), Times.Once);

			Assert.AreEqual(1, activate);
			Assert.AreEqual(2, setStartup);
			Assert.AreEqual(3, close);
			Assert.AreEqual(4, resume);
		}

		[TestMethod]
		public void MustCorrectlyRevertDisableExplorerShell()
		{
			settings.KioskMode = KioskMode.DisableExplorerShell;

			sut.Perform();
			sut.Revert();

			explorerShell.Verify(s => s.Start(), Times.Once);
		}

		[TestMethod]
		public void MustCorrectlySwitchToOtherKioskModeWhenRepeating()
		{
			var originalDesktop = new Mock<IDesktop>();
			var newDesktop = new Mock<IDesktop>();

			desktopFactory.Setup(f => f.GetCurrent()).Returns(originalDesktop.Object);
			desktopFactory.Setup(f => f.CreateNew(It.IsAny<string>())).Returns(newDesktop.Object);

			settings.KioskMode = KioskMode.CreateNewDesktop;
			sut.Perform();

			explorerShell.Verify(s => s.Terminate(), Times.Never);
			explorerShell.Verify(s => s.Start(), Times.Never);
			explorerShell.Verify(s => s.Resume(), Times.Never);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Never);
			originalDesktop.Verify(d => d.Activate(), Times.Never);

			settings.KioskMode = KioskMode.DisableExplorerShell;
			sut.Repeat();

			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Once);
			explorerShell.Verify(s => s.Start(), Times.Never);
			newDesktop.Verify(d => d.Activate(), Times.Once);
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);

			settings.KioskMode = KioskMode.CreateNewDesktop;
			sut.Repeat();

			explorerShell.Verify(s => s.Resume(), Times.Once);
			explorerShell.Verify(s => s.Terminate(), Times.Once);
			explorerShell.Verify(s => s.Suspend(), Times.Exactly(2));
			explorerShell.Verify(s => s.Start(), Times.Once);
			newDesktop.Verify(d => d.Activate(), Times.Exactly(2));
			newDesktop.Verify(d => d.Close(), Times.Once);
			originalDesktop.Verify(d => d.Activate(), Times.Once);
		}

		[TestMethod]
		public void MustDoNothingWithoutKioskMode()
		{
			settings.KioskMode = KioskMode.None;

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
			var originalDesktop = new Mock<IDesktop>();
			var newDesktop = new Mock<IDesktop>();

			settings.KioskMode = KioskMode.CreateNewDesktop;

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
		}

		[TestMethod]
		public void MustNotReinitializeDisableExplorerShellWhenRepeating()
		{
			settings.KioskMode = KioskMode.DisableExplorerShell;

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
