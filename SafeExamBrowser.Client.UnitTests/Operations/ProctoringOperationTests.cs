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
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ProctoringOperationTests
	{
		private Mock<IActionCenter> actionCenter;
		private ClientContext context;
		private Mock<IProctoringController> controller;
		private Mock<ILogger> logger;
		private Mock<INotification> notification;
		private AppSettings settings;
		private Mock<ITaskbar> taskbar;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ProctoringOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			actionCenter = new Mock<IActionCenter>();
			context = new ClientContext();
			controller = new Mock<IProctoringController>();
			logger = new Mock<ILogger>();
			notification = new Mock<INotification>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Settings = settings;
			sut = new ProctoringOperation(actionCenter.Object, context, controller.Object, logger.Object, notification.Object, taskbar.Object, uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeProctoringCorrectly()
		{
			settings.Proctoring.Enabled = true;
			settings.Proctoring.ShowTaskbarNotification = true;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			actionCenter.Verify(a => a.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Once);
			controller.Verify(c => c.Initialize(It.Is<ProctoringSettings>(s => s == settings.Proctoring)));
			notification.VerifyNoOtherCalls();
			taskbar.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Once);
			uiFactory.Verify(u => u.CreateNotificationControl(It.Is<INotification>(n => n == notification.Object), Location.ActionCenter), Times.Once);
			uiFactory.Verify(u => u.CreateNotificationControl(It.Is<INotification>(n => n == notification.Object), Location.Taskbar), Times.Once);
		}

		[TestMethod]
		public void Perform_MustDoNothingIfNotEnabled()
		{
			settings.Proctoring.Enabled = false;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			actionCenter.VerifyNoOtherCalls();
			controller.VerifyNoOtherCalls();
			notification.VerifyNoOtherCalls();
			taskbar.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Revert_MustFinalizeProctoringCorrectly()
		{
			settings.Proctoring.Enabled = true;

			Assert.AreEqual(OperationResult.Success, sut.Revert());

			actionCenter.VerifyNoOtherCalls();
			controller.Verify(c => c.Terminate(), Times.Once);
			notification.Verify(n => n.Terminate(), Times.Once);
			taskbar.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Revert_MustDoNothingIfNotEnabled()
		{
			settings.Proctoring.Enabled = false;

			Assert.AreEqual(OperationResult.Success, sut.Revert());

			actionCenter.VerifyNoOtherCalls();
			controller.VerifyNoOtherCalls();
			notification.VerifyNoOtherCalls();
			taskbar.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();
		}
	}
}
