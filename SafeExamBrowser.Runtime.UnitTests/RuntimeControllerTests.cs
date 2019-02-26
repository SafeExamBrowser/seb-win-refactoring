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
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Operations.Events;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class RuntimeControllerTests
	{
		private AppConfig appConfig;
		private Mock<IOperationSequence> bootstrapSequence;
		private Mock<IProcess> clientProcess;
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
			clientProcess = new Mock<IProcess>();
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

			sessionContext.ClientProcess = clientProcess.Object;
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
		public void ClientProcess_MustShutdownWhenClientTerminated()
		{
			StartSession();
			clientProcess.Raise(c => c.Terminated += null, -1);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			sessionSequence.Verify(s => s.TryRevert(), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void ClientProxy_MustShutdownWhenConnectionLost()
		{
			StartSession();

			clientProcess.Raise(c => c.Terminated += null, -1);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			sessionSequence.Verify(s => s.TryRevert(), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void Communication_MustProvideClientConfigurationUponRequest()
		{
			var args = new ClientConfigurationEventArgs();
			var nextAppConfig = new AppConfig();
			var nextSessionId = Guid.NewGuid();
			var nextSettings = new Settings();

			nextSession.SetupGet(s => s.AppConfig).Returns(nextAppConfig);
			nextSession.SetupGet(s => s.Id).Returns(nextSessionId);
			nextSession.SetupGet(s => s.Settings).Returns(nextSettings);
			StartSession();

			runtimeHost.Raise(r => r.ClientConfigurationNeeded += null, args);

			Assert.AreSame(nextAppConfig, args.ClientConfiguration.AppConfig);
			Assert.AreEqual(nextSessionId, args.ClientConfiguration.SessionId);
			Assert.AreSame(nextSettings, args.ClientConfiguration.Settings);
		}

		[TestMethod]
		public void Communication_MustStartNewSessionUponRequest()
		{
			var args = new ReconfigurationEventArgs { ConfigurationPath = "C:\\Some\\File\\Path.seb" };

			StartSession();
			currentSettings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			bootstrapSequence.Reset();
			sessionSequence.Reset();
			sessionSequence.Setup(s => s.TryRepeat()).Returns(OperationResult.Success);

			runtimeHost.Raise(r => r.ReconfigurationRequested += null, args);

			bootstrapSequence.VerifyNoOtherCalls();
			sessionSequence.Verify(s => s.TryPerform(), Times.Never);
			sessionSequence.Verify(s => s.TryRepeat(), Times.Once);
			sessionSequence.Verify(s => s.TryRevert(), Times.Never);

			Assert.AreEqual(sessionContext.ReconfigurationFilePath, args.ConfigurationPath);
		}

		[TestMethod]
		public void Communication_MustInformClientAboutDeniedReconfiguration()
		{
			var args = new ReconfigurationEventArgs { ConfigurationPath = "C:\\Some\\File\\Path.seb" };

			StartSession();
			currentSettings.ConfigurationMode = ConfigurationMode.Exam;
			bootstrapSequence.Reset();
			sessionSequence.Reset();

			runtimeHost.Raise(r => r.ReconfigurationRequested += null, args);

			bootstrapSequence.VerifyNoOtherCalls();
			clientProxy.Verify(c => c.InformReconfigurationDenied(It.Is<string>(s => s == args.ConfigurationPath)), Times.Once);
			sessionSequence.VerifyNoOtherCalls();

			Assert.AreNotEqual(sessionContext.ReconfigurationFilePath, args.ConfigurationPath);
		}

		[TestMethod]
		public void Communication_MustShutdownUponRequest()
		{
			StartSession();
			bootstrapSequence.Reset();
			sessionSequence.Reset();

			runtimeHost.Raise(r => r.ShutdownRequested += null);

			bootstrapSequence.VerifyNoOtherCalls();
			sessionSequence.VerifyNoOtherCalls();
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void Operations_MustRequestPasswordViaDialogOnDefaultDesktop()
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
		public void Operations_MustRequestPasswordViaClientOnNewDesktop()
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
		public void Operations_MustAbortAskingForPasswordViaClientIfDecidedByUser()
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
		public void Operations_MustNotWaitForPasswordViaClientIfCommunicationHasFailed()
		{
			var args = new PasswordRequiredEventArgs();

			currentSettings.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(new CommunicationResult(false));

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);
		}

		[TestMethod]
		public void Operations_MustShowNormalMessageBoxOnDefaultDesktop()
		{
			var args = new MessageEventArgs
			{
				Icon = MessageBoxIcon.Question,
				Message = TextKey.MessageBox_ClientConfigurationQuestion,
				Title = TextKey.MessageBox_ClientConfigurationQuestionTitle
			};

			StartSession();
			currentSettings.KioskMode = KioskMode.DisableExplorerShell;

			sessionSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<MessageBoxAction>(a => a == MessageBoxAction.Confirm),
				It.Is<MessageBoxIcon>(i => i == args.Icon),
				It.IsAny<IWindow>()), Times.Once);
			clientProxy.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Operations_MustShowMessageBoxViaClientOnNewDesktop()
		{
			var args = new MessageEventArgs
			{
				Icon = MessageBoxIcon.Question,
				Message = TextKey.MessageBox_ClientConfigurationQuestion,
				Title = TextKey.MessageBox_ClientConfigurationQuestionTitle
			};
			var reply = new MessageBoxReplyEventArgs();

			StartSession();
			currentSettings.KioskMode = KioskMode.CreateNewDesktop;

			clientProxy.Setup(c => c.ShowMessage(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<MessageBoxAction>(a => a == MessageBoxAction.Confirm),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<Guid>()))
				.Callback<string, string, MessageBoxAction, MessageBoxIcon, Guid>((m, t, a, i, id) =>
				{
					runtimeHost.Raise(r => r.MessageBoxReplyReceived += null, new MessageBoxReplyEventArgs { RequestId = id });
				})
				.Returns(new CommunicationResult(true));

			sessionSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.Never);
			clientProxy.Verify(c => c.ShowMessage(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<MessageBoxAction>(a => a == MessageBoxAction.Confirm),
				It.Is<MessageBoxIcon>(i => i == args.Icon),
				It.IsAny<Guid>()), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustRevertSessionThenBootstrapSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Once);

			Assert.AreEqual(1, session);
			Assert.AreEqual(2, bootstrap);
		}

		[TestMethod]
		public void Shutdown_MustOnlyRevertBootstrapSequenceIfNoSessionRunning()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);
			sessionContext.Current = null;

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.AreEqual(0, session);
			Assert.AreEqual(1, bootstrap);
		}

		[TestMethod]
		public void Startup_MustPerformBootstrapThenSessionSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => { session = ++order; sessionContext.Current = currentSession.Object; });
			sessionContext.Current = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Once);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.IsTrue(success);
			Assert.AreEqual(1, bootstrap);
			Assert.AreEqual(2, session);
		}

		[TestMethod]
		public void Startup_MustNotPerformSessionSequenceIfBootstrapFails()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Failed).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => { session = ++order; sessionContext.Current = currentSession.Object; });
			sessionContext.Current = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.IsFalse(success);
			Assert.AreEqual(1, bootstrap);
			Assert.AreEqual(0, session);
		}

		private void StartSession()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => sessionContext.Current = currentSession.Object);
			sessionContext.Current = null;

			sut.TryStart();
		}
	}
}
