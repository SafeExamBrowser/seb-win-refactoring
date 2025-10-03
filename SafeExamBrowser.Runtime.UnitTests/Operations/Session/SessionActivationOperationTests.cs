/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class SessionActivationOperationTests
	{
		private SessionConfiguration currentSession;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private RuntimeContext runtimeContext;
		private SessionActivationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new SessionConfiguration();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			runtimeContext = new RuntimeContext();

			nextSession.Settings = nextSettings;
			runtimeContext.Current = currentSession;
			runtimeContext.Next = nextSession;

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), runtimeContext),
				logger.Object,
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				runtimeContext,
				Mock.Of<IText>());

			sut = new SessionActivationOperation(dependencies);
		}

		[TestMethod]
		public void Perform_MustCorrectlyActivateFirstSession()
		{
			runtimeContext.Current = null;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(runtimeContext.Current, nextSession);
			Assert.IsNull(runtimeContext.Next);
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
			Assert.AreSame(runtimeContext.Current, nextSession);
			Assert.IsNull(runtimeContext.Next);
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
