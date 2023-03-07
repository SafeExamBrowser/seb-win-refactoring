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

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ProctoringWorkaroundOperationTests
	{
		private SessionContext context;
		private Mock<ILogger> logger;
		private AppSettings settings;
		private ProctoringWorkaroundOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new SessionContext();
			logger = new Mock<ILogger>();
			settings = new AppSettings();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;
			sut = new ProctoringWorkaroundOperation(logger.Object, context);
		}

		[TestMethod]
		public void Perform_MustSwitchToDisableExplorerShellIfProctoringActive()
		{
			settings.Proctoring.Enabled = true;
			settings.Security.KioskMode = KioskMode.CreateNewDesktop;

			var result = sut.Perform();

			Assert.AreEqual(KioskMode.DisableExplorerShell, settings.Security.KioskMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustDoNothingIfProctoringNotActive()
		{
			settings.Proctoring.Enabled = false;
			settings.Security.KioskMode = KioskMode.None;

			var result = sut.Perform();

			Assert.AreEqual(KioskMode.None, settings.Security.KioskMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustSwitchToDisableExplorerShellIfProctoringActive()
		{
			settings.Proctoring.Enabled = true;
			settings.Security.KioskMode = KioskMode.CreateNewDesktop;

			var result = sut.Repeat();

			Assert.AreEqual(KioskMode.DisableExplorerShell, settings.Security.KioskMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustDoNothingIfProctoringNotActive()
		{
			settings.Proctoring.Enabled = false;
			settings.Security.KioskMode = KioskMode.None;

			var result = sut.Repeat();

			Assert.AreEqual(KioskMode.None, settings.Security.KioskMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			settings.Proctoring.Enabled = true;
			settings.Security.KioskMode = KioskMode.None;

			var result = sut.Revert();

			Assert.AreEqual(KioskMode.None, settings.Security.KioskMode);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
