/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class SessionInitializationOperationTests
	{
		private AppConfig appConfig;
		private RuntimeContext context;
		private Mock<IFileSystem> fileSystem;
		private Mock<ILogger> logger;
		private Mock<IConfigurationRepository> repository;
		private SessionConfiguration session;

		private SessionInitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			context = new RuntimeContext();
			repository = new Mock<IConfigurationRepository>();
			fileSystem = new Mock<IFileSystem>();
			logger = new Mock<ILogger>();
			session = new SessionConfiguration();

			context.Next = session;
			repository.Setup(c => c.InitializeSessionConfiguration()).Returns(session);
			session.AppConfig = appConfig;

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), context),
				logger.Object,
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				context,
				Mock.Of<IText>());

			sut = new SessionInitializationOperation(dependencies, fileSystem.Object, repository.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeConfiguration()
		{
			var token = Guid.NewGuid();

			appConfig.TemporaryDirectory = @"C:\Some\Random\Path";
			session.ClientAuthenticationToken = token;

			var result = sut.Perform();

			repository.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			fileSystem.Verify(f => f.CreateDirectory(It.Is<string>(s => s == appConfig.TemporaryDirectory)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsNull(context.Current);
		}

		[TestMethod]
		public void Repeat_MustInitializeConfiguration()
		{
			var currentSession = new SessionConfiguration();
			var token = Guid.NewGuid();

			appConfig.TemporaryDirectory = @"C:\Some\Random\Path";
			session.ClientAuthenticationToken = token;
			context.Current = currentSession;

			var result = sut.Repeat();

			repository.Verify(c => c.InitializeSessionConfiguration(), Times.Once);
			fileSystem.Verify(f => f.CreateDirectory(It.Is<string>(s => s == appConfig.TemporaryDirectory)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(currentSession, context.Current);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var result = sut.Revert();

			repository.VerifyNoOtherCalls();
			fileSystem.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
