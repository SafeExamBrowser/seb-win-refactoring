/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ServerOperationTests
	{
		private AppConfig appConfig;
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<IServerProxy> server;
		private AppSettings settings;

		private ServerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			context = new ClientContext();
			logger = new Mock<ILogger>();
			server = new Mock<IServerProxy>();
			settings = new AppSettings();

			context.AppConfig = appConfig;
			context.Settings = settings;

			sut = new ServerOperation(context, logger.Object, server.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeCorrectly()
		{
			settings.SessionMode = SessionMode.Server;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			server.Verify(s => s.Initialize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.StartConnectivity(), Times.Once);
		}

		[TestMethod]
		public void Perform_MustDoNothingIfNotActive()
		{
			settings.SessionMode = SessionMode.Normal;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			server.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Revert_MustFinalizeCorrectly()
		{
			settings.SessionMode = SessionMode.Server;

			Assert.AreEqual(OperationResult.Success, sut.Revert());

			server.Verify(s => s.StopConnectivity(), Times.Once);
		}

		[TestMethod]
		public void Revert_MustDoNothingIfNotActive()
		{
			settings.SessionMode = SessionMode.Normal;

			Assert.AreEqual(OperationResult.Success, sut.Revert());

			server.VerifyNoOtherCalls();
		}
	}
}
