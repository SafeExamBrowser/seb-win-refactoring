/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class SessionInitializationOperationTests
	{
		private AppConfig appConfig;
		private Mock<IConfigurationRepository> configuration;
		private Mock<IFileSystem> fileSystem;
		private Mock<ILogger> logger;
		private Mock<IRuntimeHost> runtimeHost;
		private SessionConfiguration session;
		private SessionContext sessionContext;

		private SessionInitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			configuration = new Mock<IConfigurationRepository>();
			fileSystem = new Mock<IFileSystem>();
			logger = new Mock<ILogger>();
			runtimeHost = new Mock<IRuntimeHost>();
			session = new SessionConfiguration();
			sessionContext = new SessionContext();

			configuration.Setup(c => c.InitializeSessionConfiguration()).Returns(session);
			session.AppConfig = appConfig;
			sessionContext.Next = session;

			sut = new SessionInitializationOperation(configuration.Object, fileSystem.Object, logger.Object, runtimeHost.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustInitializeConfiguration()
		{
			var token = Guid.NewGuid();

			appConfig.TemporaryDirectory = @"C:\Some\Random\Path";
			session.ClientAuthenticationToken = token;

			var result = sut.Perform();

			configuration.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			fileSystem.Verify(f => f.CreateDirectory(It.Is<string>(s => s == appConfig.TemporaryDirectory)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsNull(sessionContext.Current);
		}

		[TestMethod]
		public void Repeat_MustInitializeConfiguration()
		{
			var currentSession = new SessionConfiguration();
			var token = Guid.NewGuid();

			appConfig.TemporaryDirectory = @"C:\Some\Random\Path";
			session.ClientAuthenticationToken = token;
			sessionContext.Current = currentSession;

			var result = sut.Repeat();

			configuration.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			fileSystem.Verify(f => f.CreateDirectory(It.Is<string>(s => s == appConfig.TemporaryDirectory)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(currentSession,sessionContext.Current);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var result = sut.Revert();

			configuration.VerifyNoOtherCalls();
			fileSystem.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
