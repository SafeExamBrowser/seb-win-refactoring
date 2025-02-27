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
		private Mock<INotification> notification1;
		private Mock<INotification> notification2;
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
			notification1 = new Mock<INotification>();
			notification2 = new Mock<INotification>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Settings = settings;
			controller.SetupGet(c => c.Notifications).Returns(new[] { notification1.Object, notification2.Object });
			sut = new ProctoringOperation(actionCenter.Object, context, controller.Object, logger.Object, taskbar.Object, uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustInitializeProctoringCorrectly()
		{
			settings.Proctoring.Enabled = true;
			settings.Proctoring.ShowTaskbarNotification = true;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			actionCenter.Verify(a => a.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Exactly(2));
			controller.Verify(c => c.Initialize(It.Is<ProctoringSettings>(s => s == settings.Proctoring)));
			notification1.VerifyNoOtherCalls();
			notification2.VerifyNoOtherCalls();
			taskbar.Verify(t => t.AddNotificationControl(It.IsAny<INotificationControl>()), Times.Exactly(2));
			uiFactory.Verify(u => u.CreateNotificationControl(It.IsAny<INotification>(), Location.ActionCenter), Times.Exactly(2));
			uiFactory.Verify(u => u.CreateNotificationControl(It.IsAny<INotification>(), Location.Taskbar), Times.Exactly(2));
		}

		[TestMethod]
		public void Perform_MustDoNothingIfNotEnabled()
		{
			settings.Proctoring.Enabled = false;

			Assert.AreEqual(OperationResult.Success, sut.Perform());

			actionCenter.VerifyNoOtherCalls();
			controller.VerifyNoOtherCalls();
			notification1.VerifyNoOtherCalls();
			notification2.VerifyNoOtherCalls();
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
			notification1.Verify(n => n.Terminate(), Times.Once);
			notification2.Verify(n => n.Terminate(), Times.Once);
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
			notification1.VerifyNoOtherCalls();
			notification2.VerifyNoOtherCalls();
			taskbar.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();
		}
	}
}
