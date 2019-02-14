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
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class ClientControllerTests
	{
		private AppConfig appConfig;
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
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private Mock<IWindowMonitor> windowMonitor;

		private ClientController sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
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
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();
			windowMonitor = new Mock<IWindowMonitor>();

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(new Mock<ISplashScreen>().Object);

			sut = new ClientController(
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
		public void MustPerformOperationsOnStartup()
		{
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);

			var success = sut.TryStart();

			operationSequence.Verify(o => o.TryPerform(), Times.Once);
			Assert.IsTrue(success);
		}

		[TestMethod]
		public void MustHandleCommunicationErrorOnStartup()
		{
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(false));

			var success = sut.TryStart();

			Assert.IsFalse(success);
		}

		[TestMethod]
		public void MustHandleStartupFailure()
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
		public void MustRevertOperationsOnShutdown()
		{
			operationSequence.Setup(o => o.TryRevert()).Returns(OperationResult.Success);
			sut.Terminate();
			operationSequence.Verify(o => o.TryRevert(), Times.Once);
		}

		[TestMethod]
		public void MustNotFailToTerminateIfDependenciesAreNull()
		{
			sut.Browser = null;
			sut.ClientHost = null;

			sut.Terminate();
		}

		[TestMethod]
		public void MustAskUserToConfirmReconfiguration()
		{
			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", new DownloadEventArgs());

			messageBox.Verify(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustDenyReconfigurationIfInExamMode()
		{
			settings.ConfigurationMode = ConfigurationMode.Exam;
			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Ok);

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", new DownloadEventArgs());
		}

		[TestMethod]
		public void MustCorrectlyHandleConfigurationDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath)), Times.Once);

			Assert.AreEqual(downloadPath, args.DownloadPath);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void MustCorrectlyHandleFailedConfigurationDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(false, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void MustCorrectlyHandleFailedReconfigurationRequest()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.DownloadDirectory = @"C:\Folder\Does\Not\Exist";
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			messageBox.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath))).Returns(new CommunicationResult(false));

			sut.TryStart();
			browserController.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>()), Times.Once);
			messageBox.Verify(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error), It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void MustHandleDisplayChangeCorrectly()
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
		public void MustHandleExplorerStartCorrectly()
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
		public void MustHandleAllowedWindowChangeCorrectly()
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
		public void MustHandleUnallowedWindowHideCorrectly()
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
		public void MustHandleUnallowedWindowCloseCorrectly()
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

		[TestMethod]
		public void MustUpdateAppConfigForSplashScreen()
		{
			var splashScreen = new Mock<ISplashScreen>();

			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(splashScreen.Object);

			sut.TryStart();
			sut.AppConfig = appConfig;

			splashScreen.VerifySet(s => s.AppConfig = appConfig, Times.Once);
		}
	}
}
