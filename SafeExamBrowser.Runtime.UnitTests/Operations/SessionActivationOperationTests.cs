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
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class SessionActivationOperationTests
	{
		private SessionConfiguration currentSession;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private SessionContext sessionContext;

		private SessionActivationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new SessionConfiguration();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			sessionContext = new SessionContext();

			nextSession.Settings = nextSettings;
			sessionContext.Current = currentSession;
			sessionContext.Next = nextSession;

			sut = new SessionActivationOperation(logger.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustCorrectlyActivateFirstSession()
		{
			sessionContext.Current = null;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(sessionContext.Current, nextSession);
			Assert.IsNull(sessionContext.Next);
		}

		[TestMethod]
		public void Perform_MustCorrectlySwitchLogSeverity()
		{
			nextSettings.LogLevel = LogLevel.Info;

			sut.Perform();

			logger.VerifySet(l => l.LogLevel = It.Is<LogLevel>(ll => ll == nextSettings.LogLevel));
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchSession()
		{
			var result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(sessionContext.Current, nextSession);
			Assert.IsNull(sessionContext.Next);
		}

		[TestMethod]
		public void Repeat_MustCorrectlySwitchLogSeverity()
		{
			nextSettings.LogLevel = LogLevel.Warning;

			sut.Perform();

			logger.VerifySet(l => l.LogLevel = It.Is<LogLevel>(ll => ll == nextSettings.LogLevel));
		}

		[TestMethod]
		public void Revert_MustAlwaysCompleteSuccessfully()
		{
			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
