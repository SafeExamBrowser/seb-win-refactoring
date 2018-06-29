/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class SessionInitializationOperationTests
	{
		private AppConfig appConfig;
		private Mock<IConfigurationRepository> configuration;
		private Mock<ILogger> logger;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<ISessionData> session;
		private SessionInitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			configuration = new Mock<IConfigurationRepository>();
			logger = new Mock<ILogger>();
			runtimeHost = new Mock<IRuntimeHost>();
			session = new Mock<ISessionData>();

			configuration.SetupGet(c => c.CurrentSession).Returns(session.Object);
			configuration.SetupGet(c => c.AppConfig).Returns(appConfig);

			sut = new SessionInitializationOperation(configuration.Object, logger.Object, runtimeHost.Object);
		}

		[TestMethod]
		public void MustInitializeConfigurationOnPerform()
		{
			var token = Guid.NewGuid();

			session.SetupGet(s => s.StartupToken).Returns(token);

			sut.Perform();

			configuration.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			runtimeHost.VerifySet(r => r.StartupToken = token, Times.Once);
		}

		[TestMethod]
		public void MustInitializeConfigurationOnRepeat()
		{
			var token = Guid.NewGuid();

			session.SetupGet(s => s.StartupToken).Returns(token);

			sut.Repeat();

			configuration.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			runtimeHost.VerifySet(r => r.StartupToken = token, Times.Once);
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			sut.Revert();

			configuration.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();
		}
	}
}
