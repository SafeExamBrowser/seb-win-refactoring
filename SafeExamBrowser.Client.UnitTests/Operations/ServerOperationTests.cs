/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ServerOperationTests
	{
		private AppConfig appConfig;
		private Mock<IActionCenter> actionCenter;
		private ClientContext context;
		private Mock<IInvigilator> invigilator;
		private Mock<ILogger> logger;
		private Mock<IServerProxy> server;
		private AppSettings settings;
		private Mock<ITaskbar> taskbar;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ServerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			actionCenter = new Mock<IActionCenter>();
			context = new ClientContext();
			invigilator = new Mock<IInvigilator>();
			logger = new Mock<ILogger>();
			server = new Mock<IServerProxy>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.AppConfig = appConfig;
			context.Settings = settings;

			sut = new ServerOperation(actionCenter.Object, context, invigilator.Object, logger.Object, server.Object, taskbar.Object, uiFactory.Object);
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
