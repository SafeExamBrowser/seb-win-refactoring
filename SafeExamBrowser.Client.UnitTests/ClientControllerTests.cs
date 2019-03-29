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
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class ClientControllerTests
	{
		private AppConfig appConfig;
		private Mock<IActionCenter> actionCenter;
		private Mock<IBrowserApplicationController> browserController;
		private Mock<IClientHost> clientHost;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<IExplorerShell> explorerShell;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IProcessMonitor> processMonitor;
		private Mock<IOperationSequence> operationSequence;
		private Mock<IRuntimeProxy> runtimeProxy;
		private Guid sessionId;
		private Settings settings;
		private Mock<Action> shutdown;
		private Mock<ITaskbar> taskbar;
		private Mock<ITerminationActivator> terminationActivator;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private Mock<IWindowMonitor> windowMonitor;

		private ClientController sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			actionCenter = new Mock<IActionCenter>();
			browserController = new Mock<IBrowserApplicationController>();
			clientHost = new Mock<IClientHost>();
			displayMonitor = new Mock<IDisplayMonitor>();
			explorerShell = new Mock<IExplorerShell>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			processMonitor = new Mock<IProcessMonitor>();
			operationSequence = new Mock<IOperationSequence>();
			runtimeProxy = new Mock<IRuntimeProxy>();
			sessionId = Guid.NewGuid();
			settings = new Settings();
			shutdown = new Mock<Action>();
			taskbar = new Mock<ITaskbar>();
			terminationActivator = new Mock<ITerminationActivator>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();
			windowMonitor = new Mock<IWindowMonitor>();

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(new Mock<ISplashScreen>().Object);

			sut = new ClientController(
				actionCenter.Object,
				displayMonitor.Object,
				explorerShell.Object,
				hashAlgorithm.Object,
				logger.Object,
				messageBox.Object,
				operationSequence.Object,
				processMonitor.Object,
				runtimeProxy.Object,
				shutdown.Object,
				taskbar.Object,
				terminationActivator.Object,
				text.Object,
				uiFactory.Object,
				windowMonitor.Object);

			sut.AppConfig = appConfig;
			sut.Browser = browserController.Object;
			sut.ClientHost = clientHost.Object;
			sut.SessionId = sessionId;
			sut.Settings = settings;
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleMessageBoxRequest()
		{
			var args = new MessageBoxRequestEventArgs
			{
				Action = MessageBoxAction.YesNo,
				Icon = MessageBoxIcon.Question,
				Message = "Some question to be answered",
				RequestId = Guid.NewGuid(),
				Title = "A Title"
			};

			messageBox.Setup(m => m.Show(
				It.Is<string>(s => s == args.Message), 
				It.Is<string>(s => s == args.Title),
				It.Is<MessageBoxAction>(a => a == args.Action),
				It.Is<MessageBoxIcon>(i => i == args.Icon),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.No);

			sut.TryStart();
			clientHost.Raise(c => c.MessageBoxRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitMessageBoxResult(
				It.Is<Guid>(g => g == args.RequestId),
				It.Is<MessageBoxResult>(r => r == MessageBoxResult.No)), Times.Once);
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
			var result = new Mock<IPasswordDialogResult>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(result.Object);
			result.SetupGet(r => r.Password).Returns("blubb");
			result.SetupGet(r => r.Success).Returns(true);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			sut.TryStart();
			clientHost.Raise(c => c.PasswordRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitPassword(
				It.Is<Guid>(g => g == args.RequestId),
				It.Is<bool>(b => b == result.Object.Success),
				It.Is<string>(s => s == result.Object.Password)), Times.Once);
		}

		[TestMethod]
		public void Communication_MustInformUserAboutDeniedReconfiguration()
		{
			var args = new ReconfigurationEventArgs
			{
				ConfigurationPath = @"C:\Some\File\Path.seb"
			};

			sut.TryStart();
			clientHost.Raise(c => c.ReconfigurationDenied += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyInitiateShutdown()
		{
			sut.TryStart();
			clientHost.Raise(c => c.Shutdown += null);

			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void Communication_MustShutdownOnLostConnection()
		{
			sut.TryStart();
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
		public void DisplayMonitor_MustHandleDisplayChangeCorrectly()
		{
			var order = 0;
			var workingArea = 0;
			var taskbar = 0;

			displayMonitor.Setup(w => w.InitializePrimaryDisplay(this.taskbar.Object.GetAbsoluteHeight())).Callback(() => workingArea = ++order);
			this.taskbar.Setup(t => t.InitializeBounds()).Callback(() => taskbar = ++order);

			sut.TryStart();
			displayMonitor.Raise(d => d.DisplayChanged += null);

			displayMonitor.Verify(w => w.InitializePrimaryDisplay(this.taskbar.Object.GetAbsoluteHeight()), Times.Once);
			this.taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(workingArea == 1);
			Assert.IsTrue(taskbar == 2);
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
			var splashScreen = new Mock<ISplashScreen>();

			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(splashScreen.Object);

			sut.TryStart();
			operationSequence.Raise(o => o.ProgressChanged += null, args);

			splashScreen.Verify(s => s.SetValue(It.Is<int>(i => i == args.CurrentValue)), Times.Once);
			splashScreen.Verify(s => s.SetIndeterminate(), Times.Once);
			splashScreen.Verify(s => s.SetMaxValue(It.Is<int>(i => i == args.MaxValue)), Times.Once);
			splashScreen.Verify(s => s.Progress(), Times.Once);
			splashScreen.Verify(s => s.Regress(), Times.Once);
		}

		[TestMethod]
		public void Operations_MustUpdateStatus()
		{
			var key = TextKey.OperationStatus_EmptyClipboard;
			var splashScreen = new Mock<ISplashScreen>();

			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(splashScreen.Object);

			sut.TryStart();
			operationSequence.Raise(o => o.StatusChanged += null, key);

			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(k => k == key), It.IsAny<bool>()), Times.Once);
		}

		[TestMethod]
		public void ProcessMonitor_MustHandleExplorerStartCorrectly()
		{
			var order = 0;
			var shell = 0;
			var workingArea = 0;
			var bounds = 0;

			explorerShell.Setup(e => e.Terminate()).Callback(() => shell = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(taskbar.Object.GetAbsoluteHeight())).Callback(() => workingArea = ++order);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => bounds = ++order);

			sut.TryStart();
			processMonitor.Raise(p => p.ExplorerStarted += null);

			explorerShell.Verify(p => p.Terminate(), Times.Once);
			displayMonitor.Verify(w => w.InitializePrimaryDisplay(taskbar.Object.GetAbsoluteHeight()), Times.Once);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(shell == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(bounds == 3);
		}

		[TestMethod]
		public void Reconfiguration_MustAskUserToConfirm()
		{
			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", new DownloadEventArgs());

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Reconfiguration_MustNotStartIfNotWantedByUser()
		{
			var args = new DownloadEventArgs();

			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.No);

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);

			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustDenyIfInExamMode()
		{
			settings.ConfigurationMode = ConfigurationMode.Exam;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Ok);

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", new DownloadEventArgs());
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath)), Times.Once);

			Assert.AreEqual(downloadPath, args.DownloadPath);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(false, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedRequest()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(false));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>()), Times.Once);
			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustAskUserToConfirm()
		{
			var args = new System.ComponentModel.CancelEventArgs();

			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
			Assert.IsFalse(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustNotInitiateIfNotWishedByUser()
		{
			var args = new System.ComponentModel.CancelEventArgs();

			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.No);

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Never);
			Assert.IsTrue(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustCloseActionCenterAndTaskbar()
		{
			sut.Terminate();
			actionCenter.Verify(a => a.Close(), Times.Once);
			taskbar.Verify(o => o.Close(), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustShowErrorMessageOnCommunicationFailure()
		{
			var args = new System.ComponentModel.CancelEventArgs();

			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(false));

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustAskUserForQuitPassword()
		{
			var args = new System.ComponentModel.CancelEventArgs();
			var dialog = new Mock<IPasswordDialog>();
			var dialogResult = new Mock<IPasswordDialogResult>();
			var password = "blobb";

			settings.QuitPasswordHash = "1234";
			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(dialogResult.Object);
			dialogResult.SetupGet(r => r.Password).Returns(password);
			dialogResult.SetupGet(r => r.Success).Returns(true);
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(s => s == password))).Returns(settings.QuitPasswordHash);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);

			Assert.IsFalse(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustNotInitiateIfQuitPasswordIncorrect()
		{
			var args = new System.ComponentModel.CancelEventArgs();
			var dialog = new Mock<IPasswordDialog>();
			var dialogResult = new Mock<IPasswordDialogResult>();

			settings.QuitPasswordHash = "1234";
			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(dialogResult.Object);
			dialogResult.SetupGet(r => r.Password).Returns("blobb");
			dialogResult.SetupGet(r => r.Success).Returns(true);
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.IsAny<string>())).Returns("9876");
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Warning),
				It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Never);

			Assert.IsTrue(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustRevertOperations()
		{
			operationSequence.Setup(o => o.TryRevert()).Returns(OperationResult.Success);
			sut.Terminate();
			operationSequence.Verify(o => o.TryRevert(), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustNotFailIfDependenciesAreNull()
		{
			sut.Browser = null;
			sut.ClientHost = null;

			sut.Terminate();
		}

		[TestMethod]
		public void Startup_MustPerformOperations()
		{
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);

			var success = sut.TryStart();

			operationSequence.Verify(o => o.TryPerform(), Times.Once);
			Assert.IsTrue(success);
		}

		[TestMethod]
		public void Startup_MustHandleCommunicationError()
		{
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(false));

			var success = sut.TryStart();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void Startup_MustHandleFailure()
		{
			var success = true;

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Failed);
			success = sut.TryStart();

			Assert.IsFalse(success);

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Aborted);
			success = sut.TryStart();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void Startup_MustUpdateAppConfigForSplashScreen()
		{
			var splashScreen = new Mock<ISplashScreen>();

			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(splashScreen.Object);

			sut.TryStart();
			sut.AppConfig = appConfig;

			splashScreen.VerifySet(s => s.AppConfig = appConfig, Times.Once);
		}

		[TestMethod]
		public void Startup_MustCorrectlyHandleTaskbar()
		{
			settings.Taskbar.EnableTaskbar = true;
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			sut.TryStart();

			taskbar.Verify(t => t.Show(), Times.Once);

			taskbar.Reset();
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Aborted);
			sut.TryStart();

			taskbar.Verify(t => t.Show(), Times.Never);
		}

		[TestMethod]
		public void WindowMonitor_MustHandleAllowedWindowChangeCorrectly()
		{
			var window = new IntPtr(12345);

			processMonitor.Setup(p => p.BelongsToAllowedProcess(window)).Returns(true);

			sut.TryStart();
			windowMonitor.Raise(w => w.WindowChanged += null, window);

			processMonitor.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitor.Verify(w => w.Hide(window), Times.Never);
			windowMonitor.Verify(w => w.Close(window), Times.Never);
		}

		[TestMethod]
		public void WindowMonitor_MustHandleUnallowedWindowHideCorrectly()
		{
			var order = 0;
			var belongs = 0;
			var hide = 0;
			var window = new IntPtr(12345);

			processMonitor.Setup(p => p.BelongsToAllowedProcess(window)).Returns(false).Callback(() => belongs = ++order);
			windowMonitor.Setup(w => w.Hide(window)).Returns(true).Callback(() => hide = ++order);

			sut.TryStart();
			windowMonitor.Raise(w => w.WindowChanged += null, window);

			processMonitor.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitor.Verify(w => w.Hide(window), Times.Once);
			windowMonitor.Verify(w => w.Close(window), Times.Never);

			Assert.IsTrue(belongs == 1);
			Assert.IsTrue(hide == 2);
		}

		[TestMethod]
		public void WindowMonitor_MustHandleUnallowedWindowCloseCorrectly()
		{
			var order = 0;
			var belongs = 0;
			var hide = 0;
			var close = 0;
			var window = new IntPtr(12345);

			processMonitor.Setup(p => p.BelongsToAllowedProcess(window)).Returns(false).Callback(() => belongs = ++order);
			windowMonitor.Setup(w => w.Hide(window)).Returns(false).Callback(() => hide = ++order);
			windowMonitor.Setup(w => w.Close(window)).Callback(() => close = ++order);

			sut.TryStart();
			windowMonitor.Raise(w => w.WindowChanged += null, window);

			processMonitor.Verify(p => p.BelongsToAllowedProcess(window), Times.Once);
			windowMonitor.Verify(w => w.Hide(window), Times.Once);
			windowMonitor.Verify(w => w.Close(window), Times.Once);

			Assert.IsTrue(belongs == 1);
			Assert.IsTrue(hide == 2);
			Assert.IsTrue(close == 3);
		}
	}
}
