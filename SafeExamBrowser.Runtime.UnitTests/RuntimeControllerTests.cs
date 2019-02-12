/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.Runtime.Operations.Events;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class RuntimeControllerTests
	{
		private AppConfig appConfig;
		private Mock<IOperationSequence> bootstrapSequence;
		private Mock<IClientProxy> clientProxy;
		private Mock<ISessionConfiguration> currentSession;
		private Settings currentSettings;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<ISessionConfiguration> nextSession;
		private Settings nextSettings;
		private Mock<Action> shutdown;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private RuntimeController sut;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IServiceProxy> service;
		private SessionContext sessionContext;
		private Mock<IRepeatableOperationSequence> sessionSequence;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			bootstrapSequence = new Mock<IOperationSequence>();
			clientProxy = new Mock<IClientProxy>();
			currentSession = new Mock<ISessionConfiguration>();
			currentSettings = new Settings();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			nextSession = new Mock<ISessionConfiguration>();
			nextSettings = new Settings();
			runtimeHost = new Mock<IRuntimeHost>();
			service = new Mock<IServiceProxy>();
			sessionContext = new SessionContext();
			sessionSequence = new Mock<IRepeatableOperationSequence>();
			shutdown = new Mock<Action>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			currentSession.SetupGet(s => s.Settings).Returns(currentSettings);
			nextSession.SetupGet(s => s.Settings).Returns(nextSettings);

			sessionContext.ClientProxy = clientProxy.Object;
			sessionContext.Current = currentSession.Object;
			sessionContext.Next = nextSession.Object;

			uiFactory.Setup(u => u.CreateRuntimeWindow(It.IsAny<AppConfig>())).Returns(new Mock<IRuntimeWindow>().Object);
			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(new Mock<ISplashScreen>().Object);

			sut = new RuntimeController(
				appConfig,
				logger.Object,
				messageBox.Object,
				bootstrapSequence.Object,
				sessionSequence.Object,
				runtimeHost.Object,
				service.Object,
				sessionContext,
				shutdown.Object,
				text.Object,
				uiFactory.Object);
		}

		[TestMethod]
		public void MustRequestPasswordViaDialogOnDefaultDesktop()
		{
			var args = new PasswordRequiredEventArgs();
			var password = "test1234";
			var passwordDialog = new Mock<IPasswordDialog>();
			var result = new Mock<IPasswordDialogResult>();

			currentSettings.KioskMode = KioskMode.DisableExplorerShell;
			passwordDialog.Setup(p => p.Show(It.IsAny<IWindow>())).Returns(result.Object);
			result.SetupGet(r => r.Password).Returns(password);
			result.SetupGet(r => r.Success).Returns(true);
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordDialog.Object);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.VerifyNoOtherCalls();
			passwordDialog.Verify(p => p.Show(It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			Assert.AreEqual(true, args.Success);
			Assert.AreEqual(password, args.Password);
		}

		[TestMethod]
		public void MustRequestPasswordViaClientOnNewDesktop()
		{
			var args = new PasswordRequiredEventArgs();
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = true });
			});

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(passwordReceived);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void MustAbortAskingForPasswordViaClientIfDecidedByUser()
		{
			var args = new PasswordRequiredEventArgs();
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = false });
			});

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(passwordReceived);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void MustNotWaitForPasswordViaClientIfCommunicationHasFailed()
		{
			var args = new PasswordRequiredEventArgs();

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(new CommunicationResult(false));

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);
		}
	}
}
