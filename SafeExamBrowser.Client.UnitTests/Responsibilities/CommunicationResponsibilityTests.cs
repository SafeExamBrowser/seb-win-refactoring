/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class CommunicationResponsibilityTests
	{
		private Mock<IClientHost> clientHost;
		private ClientContext context;
		private Mock<ICoordinator> coordinator;
		private Mock<IMessageBox> messageBox;
		private Mock<IRuntimeProxy> runtimeProxy;
		private Mock<Action> shutdown;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private CommunicationResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();

			clientHost = new Mock<IClientHost>();
			context = new ClientContext();
			coordinator = new Mock<ICoordinator>();
			messageBox = new Mock<IMessageBox>();
			runtimeProxy = new Mock<IRuntimeProxy>();
			shutdown = new Mock<Action>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.ClientHost = clientHost.Object;

			sut = new CommunicationResponsibility(
				context,
				coordinator.Object,
				logger.Object,
				messageBox.Object,
				runtimeProxy.Object,
				shutdown.Object,
				splashScreen.Object,
				text.Object,
				uiFactory.Object);

			sut.Assume(ClientTask.RegisterEvents);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleExamSelection()
		{
			var args = new ExamSelectionRequestEventArgs
			{
				Exams = new List<(string id, string lms, string name, string url)> { ("", "", "", "") },
				RequestId = Guid.NewGuid()
			};
			var dialog = new Mock<IExamSelectionDialog>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { Success = true });
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			clientHost.Raise(c => c.ExamSelectionRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitExamSelectionResult(It.Is<Guid>(g => g == args.RequestId), true, null), Times.Once);
			uiFactory.Verify(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>()), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleMessageBoxRequest()
		{
			var args = new MessageBoxRequestEventArgs
			{
				Action = (int) MessageBoxAction.YesNo,
				Icon = (int) MessageBoxIcon.Question,
				Message = "Some question to be answered",
				RequestId = Guid.NewGuid(),
				Title = "A Title"
			};

			messageBox.Setup(m => m.Show(
				It.Is<string>(s => s == args.Message),
				It.Is<string>(s => s == args.Title),
				It.Is<MessageBoxAction>(a => a == (MessageBoxAction) args.Action),
				It.Is<MessageBoxIcon>(i => i == (MessageBoxIcon) args.Icon),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.No);

			clientHost.Raise(c => c.MessageBoxRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitMessageBoxResult(
				It.Is<Guid>(g => g == args.RequestId),
				It.Is<int>(r => r == (int) MessageBoxResult.No)), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandlePasswordRequest()
		{
			var args = new PasswordRequestEventArgs
			{
				Purpose = PasswordRequestPurpose.LocalSettings,
				RequestId = Guid.NewGuid()
			};
			var dialog = new Mock<IPasswordDialog>();
			var result = new PasswordDialogResult { Password = "blubb", Success = true };

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(result);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			clientHost.Raise(c => c.PasswordRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitPassword(
				It.Is<Guid>(g => g == args.RequestId),
				It.Is<bool>(b => b == result.Success),
				It.Is<string>(s => s == result.Password)), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleAbortedReconfiguration()
		{
			clientHost.Raise(c => c.ReconfigurationAborted += null);

			splashScreen.Verify(s => s.Hide(), Times.AtLeastOnce);
		}

		[TestMethod]
		public void Communication_MustInformUserAboutDeniedReconfiguration()
		{
			var args = new ReconfigurationEventArgs
			{
				ConfigurationPath = @"C:\Some\File\Path.seb"
			};

			clientHost.Raise(c => c.ReconfigurationDenied += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleServerCommunicationFailure()
		{
			var args = new ServerFailureActionRequestEventArgs { RequestId = Guid.NewGuid() };
			var dialog = new Mock<IServerFailureDialog>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ServerFailureDialogResult());
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(dialog.Object);

			clientHost.Raise(c => c.ServerFailureActionRequested += null, args);

			runtimeProxy.Verify(r => r.SubmitServerFailureActionResult(It.Is<Guid>(g => g == args.RequestId), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
			uiFactory.Verify(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyInitiateShutdown()
		{
			clientHost.Raise(c => c.Shutdown += null);

			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void Communication_MustShutdownOnLostConnection()
		{
			runtimeProxy.Raise(p => p.ConnectionLost += null);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void MustNotFailIfDependencyIsNull()
		{
			context.ClientHost = null;
			sut.Assume(ClientTask.DeregisterEvents);
		}
	}
}
