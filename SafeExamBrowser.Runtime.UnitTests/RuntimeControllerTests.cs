/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class RuntimeControllerTests
	{
		private AppConfig appConfig;
		private Mock<IOperationSequence> bootstrapSequence;
		private Mock<IProcess> clientProcess;
		private Mock<IClientProxy> clientProxy;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<IServiceProxy> service;
		private SessionContext sessionContext;
		private Mock<IRepeatableOperationSequence> sessionSequence;
		private Mock<Action> shutdown;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private RuntimeController sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			bootstrapSequence = new Mock<IOperationSequence>();
			clientProcess = new Mock<IProcess>();
			clientProxy = new Mock<IClientProxy>();
			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			runtimeHost = new Mock<IRuntimeHost>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			service = new Mock<IServiceProxy>();
			sessionContext = new SessionContext();
			sessionSequence = new Mock<IRepeatableOperationSequence>();
			shutdown = new Mock<Action>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;

			sessionContext.ClientProcess = clientProcess.Object;
			sessionContext.ClientProxy = clientProxy.Object;
			sessionContext.Current = currentSession;
			sessionContext.Next = nextSession;

			uiFactory.Setup(u => u.CreateRuntimeWindow(It.IsAny<AppConfig>())).Returns(new Mock<IRuntimeWindow>().Object);
			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(new Mock<ISplashScreen>().Object);

			sut = new RuntimeController(
				appConfig,
				logger.Object,
				messageBox.Object,
				bootstrapSequence.Object,
				sessionSequence.Object,
				runtimeHost.Object,
				runtimeWindow.Object,
				service.Object,
				sessionContext,
				shutdown.Object,
				splashScreen.Object,
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
			clientProxy.Raise(c => c.ConnectionLost += null);

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
			var nextSettings = new AppSettings();

			nextSession.AppConfig = nextAppConfig;
			nextSession.SessionId = nextSessionId;
			nextSession.Settings = nextSettings;
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
		public void Communication_MustInformClientAboutAbortedReconfiguration()
		{
			StartSession();
			sessionSequence.Reset();
			sessionSequence.Setup(s => s.TryRepeat()).Returns(OperationResult.Aborted);

			runtimeHost.Raise(r => r.ReconfigurationRequested += null, new ReconfigurationEventArgs());

			clientProxy.Verify(c => c.InformReconfigurationAborted(), Times.Once);
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
		public void Operations_MustAllowToAbortStartupForClientConfiguration()
		{
			var args = new ConfigurationCompletedEventArgs();

			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			Assert.IsTrue(args.AbortStartup);
		}

		[TestMethod]
		public void Operations_MustRequestServerExamSelectionViaDialogOnDefaultDesktop()
		{
			var args = new ExamSelectionEventArgs(Enumerable.Empty<Exam>());
			var examSelectionDialog = new Mock<IExamSelectionDialog>();
			var result = new ExamSelectionDialogResult { SelectedExam = new Exam(), Success = true };

			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			examSelectionDialog.Setup(p => p.Show(It.IsAny<IWindow>())).Returns(result);
			uiFactory.Setup(u => u.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examSelectionDialog.Object);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.VerifyNoOtherCalls();
			examSelectionDialog.Verify(p => p.Show(It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>()), Times.Once);

			Assert.AreEqual(true, args.Success);
			Assert.AreEqual(result.SelectedExam, args.SelectedExam);
		}

		[TestMethod]
		public void Operations_MustRequestServerExamSelectionViaClientOnNewDesktop()
		{
			var args = new ExamSelectionEventArgs(new[] { new Exam { Id = "abc1234" } });
			var examSelectionReceived = new Action<IEnumerable<(string, string, string, string)>, Guid>((e, id) =>
			{
				runtimeHost.Raise(r => r.ExamSelectionReceived += null, new ExamSelectionReplyEventArgs
				{
					RequestId = id,
					SelectedExamId = "abc1234",
					Success = true
				});
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.RequestExamSelection(It.IsAny<IEnumerable<(string, string, string, string)>>(), It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(examSelectionReceived);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.Verify(c => c.RequestExamSelection(It.IsAny<IEnumerable<(string, string, string, string)>>(), It.IsAny<Guid>()), Times.Once);
			uiFactory.Verify(u => u.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>()), Times.Never);

			Assert.AreEqual("abc1234", args.SelectedExam.Id);
		}

		[TestMethod]
		public void Operations_MustRequestPasswordViaDialogOnDefaultDesktop()
		{
			var args = new PasswordRequiredEventArgs();
			var passwordDialog = new Mock<IPasswordDialog>();
			var result = new PasswordDialogResult { Password = "test1234", Success = true };

			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			passwordDialog.Setup(p => p.Show(It.IsAny<IWindow>())).Returns(result);
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordDialog.Object);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.VerifyNoOtherCalls();
			passwordDialog.Verify(p => p.Show(It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			Assert.AreEqual(true, args.Success);
			Assert.AreEqual(result.Password, args.Password);
		}

		[TestMethod]
		public void Operations_MustRequestPasswordViaClientOnNewDesktop()
		{
			var args = new PasswordRequiredEventArgs();
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { Password = "test", RequestId = id, Success = true });
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(new CommunicationResult(true)).Callback(passwordReceived);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert.AreEqual("test", args.Password);
		}

		[TestMethod]
		public void Operations_MustRequestServerFailureActionViaDialogOnDefaultDesktop()
		{
			var args = new ServerFailureEventArgs(default(string), default(bool));
			var failureDialog = new Mock<IServerFailureDialog>();
			var result = new ServerFailureDialogResult { Fallback = true, Success = true };

			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			failureDialog.Setup(p => p.Show(It.IsAny<IWindow>())).Returns(result);
			uiFactory.Setup(u => u.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(failureDialog.Object);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.VerifyNoOtherCalls();
			failureDialog.Verify(p => p.Show(It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);

			Assert.AreEqual(result.Abort, args.Abort);
			Assert.AreEqual(result.Fallback, args.Fallback);
			Assert.AreEqual(result.Retry, args.Retry);
		}

		[TestMethod]
		public void Operations_MustRequestServerFailureActionViaClientOnNewDesktop()
		{
			var args = new ServerFailureEventArgs(default(string), default(bool));
			var failureActionReceived = new Action<string, bool, Guid>((m, f, id) =>
			{
				runtimeHost.Raise(r => r.ServerFailureActionReceived += null, new ServerFailureActionReplyEventArgs
				{
					RequestId = id,
					Fallback = true
				});
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.RequestServerFailureAction(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(failureActionReceived);

			sut.TryStart();
			sessionSequence.Raise(s => s.ActionRequired += null, args);

			clientProxy.Verify(c => c.RequestServerFailureAction(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>()), Times.Once);
			uiFactory.Verify(u => u.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

			Assert.IsFalse(args.Abort);
			Assert.IsTrue(args.Fallback);
			Assert.IsFalse(args.Retry);
		}

		[TestMethod]
		public void Operations_MustAbortAskingForPasswordViaClientIfDecidedByUser()
		{
			var args = new PasswordRequiredEventArgs();
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = false });
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
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

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
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
			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;

			sessionSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<MessageBoxAction>(a => a == MessageBoxAction.Ok),
				It.Is<MessageBoxIcon>(i => i == args.Icon),
				It.IsAny<IWindow>()), Times.Once);
			clientProxy.VerifyAdd(p => p.ConnectionLost += It.IsAny<CommunicationEventHandler>());
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
			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;

			clientProxy.Setup(c => c.ShowMessage(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<int>(a => a == (int) MessageBoxAction.Ok),
				It.IsAny<int>(),
				It.IsAny<Guid>()))
				.Callback<string, string, int, int, Guid>((m, t, a, i, id) =>
				{
					runtimeHost.Raise(r => r.MessageBoxReplyReceived += null, new MessageBoxReplyEventArgs { RequestId = id });
				})
				.Returns(new CommunicationResult(true));

			sessionSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.Never);
			clientProxy.Verify(c => c.ShowMessage(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.Is<int>(a => a == (int) MessageBoxAction.Ok),
				It.Is<int>(i => i == (int) args.Icon),
				It.IsAny<Guid>()), Times.Once);
		}

		[TestMethod]
		public void Operations_MustUpdateProgress()
		{
			var args = new ProgressChangedEventArgs
			{
				CurrentValue = 23,
				IsIndeterminate = true,
				MaxValue = 150,
				Progress = true,
				Regress = true
			};

			sut.TryStart();
			sessionSequence.Raise(o => o.ProgressChanged += null, args);

			runtimeWindow.Verify(s => s.SetValue(It.Is<int>(i => i == args.CurrentValue)), Times.Once);
			runtimeWindow.Verify(s => s.SetIndeterminate(), Times.Once);
			runtimeWindow.Verify(s => s.SetMaxValue(It.Is<int>(i => i == args.MaxValue)), Times.Once);
			runtimeWindow.Verify(s => s.Progress(), Times.Once);
			runtimeWindow.Verify(s => s.Regress(), Times.Once);
		}

		[TestMethod]
		public void Operations_MustUpdateStatus()
		{
			var key = TextKey.OperationStatus_EmptyClipboard;

			sut.TryStart();
			sessionSequence.Raise(o => o.StatusChanged += null, key);

			runtimeWindow.Verify(s => s.UpdateStatus(It.Is<TextKey>(k => k == key), It.IsAny<bool>()), Times.Once);
		}

		[TestMethod]
		public void Session_MustHideRuntimeWindowWhenUsingDisableExplorerShell()
		{
			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			StartSession();
			runtimeWindow.Verify(w => w.Hide(), Times.AtLeastOnce);

			runtimeWindow.Reset();
			sessionSequence.Reset();

			sessionSequence.Setup(b => b.TryRepeat()).Returns(OperationResult.Aborted);
			runtimeHost.Raise(h => h.ReconfigurationRequested += null, new ReconfigurationEventArgs());
			runtimeWindow.Verify(w => w.Hide(), Times.AtLeastOnce);
		}

		[TestMethod]
		public void Session_MustShowMessageBoxOnFailure()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Failed);
			sessionContext.Current = null;
			sut.TryStart();
			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.AtLeastOnce);

			StartSession();
			messageBox.Reset();
			sessionSequence.Reset();

			sessionSequence.Setup(b => b.TryRepeat()).Returns(OperationResult.Failed);
			runtimeHost.Raise(h => h.ReconfigurationRequested += null, new ReconfigurationEventArgs());
			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.AtLeastOnce);
		}

		[TestMethod]
		public void ServiceProxy_MustShutdownWhenConnectionLostAndMandatory()
		{
			currentSettings.Service.Policy = ServicePolicy.Mandatory;

			StartSession();
			service.Raise(c => c.ConnectionLost += null);

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
		public void ServiceProxy_MustNotShutdownWhenConnectionLostAndNotMandatory()
		{
			currentSettings.Service.Policy = ServicePolicy.Optional;

			StartSession();
			service.Raise(c => c.ConnectionLost += null);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Never);
			sessionSequence.Verify(s => s.TryRevert(), Times.Never);
			shutdown.Verify(s => s(), Times.Never);
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
		public void Shutdown_MustIndicateFailureToUser()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Failed).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);

			sut.Terminate();

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.AtLeastOnce);
		}

		[TestMethod]
		public void Startup_MustPerformBootstrapThenSessionSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => { session = ++order; sessionContext.Current = currentSession; });
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
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => { session = ++order; sessionContext.Current = currentSession; });
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

		[TestMethod]
		public void Startup_MustTerminateOnSessionStartFailure()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Failed).Callback(() => sessionContext.Current = currentSession);
			sessionContext.Current = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Once);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Once);

			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustNotTerminateOnSessionStartAbortion()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Aborted).Callback(() => sessionContext.Current = currentSession);
			sessionContext.Current = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Once);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			shutdown.Verify(s => s(), Times.Never);
		}

		[TestMethod]
		public void Startup_MustUpdateProgressForBootstrapSequence()
		{
			var args = new ProgressChangedEventArgs
			{
				CurrentValue = 12,
				IsIndeterminate = true,
				MaxValue = 100,
				Progress = true,
				Regress = true
			};

			bootstrapSequence
				.Setup(b => b.TryPerform())
				.Returns(OperationResult.Success)
				.Callback(() => { bootstrapSequence.Raise(s => s.ProgressChanged += null, args); });

			var success = sut.TryStart();

			splashScreen.Verify(s => s.SetValue(It.Is<int>(i => i == args.CurrentValue)), Times.Once);
			splashScreen.Verify(s => s.SetIndeterminate(), Times.Once);
			splashScreen.Verify(s => s.SetMaxValue(It.Is<int>(i => i == args.MaxValue)), Times.Once);
			splashScreen.Verify(s => s.Progress(), Times.Once);
			splashScreen.Verify(s => s.Regress(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustUpdateStatusForBootstrapSequence()
		{
			var key = TextKey.OperationStatus_InitializeBrowser;

			bootstrapSequence
				.Setup(b => b.TryPerform())
				.Returns(OperationResult.Success)
				.Callback(() => { bootstrapSequence.Raise(s => s.StatusChanged += null, key); });

			var success = sut.TryStart();

			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(k => k == key), true), Times.Once);
		}

		private void StartSession()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success).Callback(() => sessionContext.Current = currentSession);
			sessionContext.Current = null;

			sut.TryStart();
		}
	}
}
