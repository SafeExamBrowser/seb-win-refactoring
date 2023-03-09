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
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class ClientControllerTests
	{
		private AppConfig appConfig;
		private Mock<IActionCenter> actionCenter;
		private Mock<IApplicationMonitor> applicationMonitor;
		private Mock<IBrowserApplication> browser;
		private Mock<IClientHost> clientHost;
		private ClientContext context;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<IExplorerShell> explorerShell;
		private Mock<IFileSystemDialog> fileSystemDialog;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<IIntegrityModule> integrityModule;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IOperationSequence> operationSequence;
		private Mock<IRegistry> registry;
		private Mock<IRuntimeProxy> runtimeProxy;
		private Mock<IServerProxy> server;
		private Guid sessionId;
		private AppSettings settings;
		private Mock<Action> shutdown;
		private Mock<ISplashScreen> splashScreen;
		private Mock<ISystemMonitor> systemMonitor;
		private Mock<ITaskbar> taskbar;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ClientController sut;

		[TestInitialize]
		public void Initialize()
		{
			var valid = true;

			appConfig = new AppConfig();
			actionCenter = new Mock<IActionCenter>();
			applicationMonitor = new Mock<IApplicationMonitor>();
			browser = new Mock<IBrowserApplication>();
			clientHost = new Mock<IClientHost>();
			context = new ClientContext();
			displayMonitor = new Mock<IDisplayMonitor>();
			explorerShell = new Mock<IExplorerShell>();
			fileSystemDialog = new Mock<IFileSystemDialog>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			integrityModule = new Mock<IIntegrityModule>();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			operationSequence = new Mock<IOperationSequence>();
			registry = new Mock<IRegistry>();
			runtimeProxy = new Mock<IRuntimeProxy>();
			server = new Mock<IServerProxy>();
			sessionId = Guid.NewGuid();
			settings = new AppSettings();
			shutdown = new Mock<Action>();
			splashScreen = new Mock<ISplashScreen>();
			systemMonitor = new Mock<ISystemMonitor>();
			taskbar = new Mock<ITaskbar>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			integrityModule.Setup(m => m.TryVerifySessionIntegrity(It.IsAny<string>(), It.IsAny<string>(), out valid)).Returns(true);
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(new Mock<ISplashScreen>().Object);

			sut = new ClientController(
				actionCenter.Object,
				applicationMonitor.Object,
				context,
				displayMonitor.Object,
				explorerShell.Object,
				fileSystemDialog.Object,
				hashAlgorithm.Object,
				logger.Object,
				messageBox.Object,
				operationSequence.Object,
				registry.Object,
				runtimeProxy.Object,
				shutdown.Object,
				splashScreen.Object,
				systemMonitor.Object,
				taskbar.Object,
				text.Object,
				uiFactory.Object);

			context.AppConfig = appConfig;
			context.Browser = browser.Object;
			context.ClientHost = clientHost.Object;
			context.IntegrityModule = integrityModule.Object;
			context.Server = server.Object;
			context.SessionId = sessionId;
			context.Settings = settings;
		}

		[TestMethod]
		public void ApplicationMonitor_MustCorrectlyHandleExplorerStartWithTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 30;
			var order = 0;
			var shell = 0;
			var workingArea = 0;

			settings.Taskbar.EnableTaskbar = true;

			actionCenter.Setup(a => a.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			explorerShell.Setup(e => e.Terminate()).Callback(() => shell = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == height))).Callback(() => workingArea = ++order);
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			sut.TryStart();
			applicationMonitor.Raise(a => a.ExplorerStarted += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			explorerShell.Verify(e => e.Terminate(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Never);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Once);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Once);

			Assert.IsTrue(shell == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(boundsActionCenter == 3);
			Assert.IsTrue(boundsTaskbar == 4);
		}

		[TestMethod]
		public void ApplicationMonitor_MustCorrectlyHandleExplorerStartWithoutTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 30;
			var order = 0;
			var shell = 0;
			var workingArea = 0;

			settings.Taskbar.EnableTaskbar = false;

			actionCenter.Setup(a => a.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			explorerShell.Setup(e => e.Terminate()).Callback(() => shell = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == 0))).Callback(() => workingArea = ++order);
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			sut.TryStart();
			applicationMonitor.Raise(a => a.ExplorerStarted += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			explorerShell.Verify(e => e.Terminate(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Never);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Never);

			Assert.IsTrue(shell == 1);
			Assert.IsTrue(workingArea == 2);
			Assert.IsTrue(boundsActionCenter == 3);
			Assert.IsTrue(boundsTaskbar == 4);
		}

		[TestMethod]
		public void ApplicationMonitor_MustPermitApplicationIfChosenByUserAfterFailedTermination()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtimeProxy.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>()))
				.Returns(lockScreen.Object)
				.Callback<string, string, IEnumerable<LockScreenOption>>((m, t, o) => result.OptionId = o.First().Id);

			sut.TryStart();
			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Never);
		}

		[TestMethod]
		public void ApplicationMonitor_MustRequestShutdownIfChosenByUserAfterFailedTermination()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtimeProxy.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>()))
				.Returns(lockScreen.Object)
				.Callback<string, string, IEnumerable<LockScreenOption>>((m, t, o) => result.OptionId = o.Last().Id);

			sut.TryStart();
			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void ApplicationMonitor_MustShowLockScreenIfTerminationFailed()
		{
			var activator1 = new Mock<IActivator>();
			var activator2 = new Mock<IActivator>();
			var activator3 = new Mock<IActivator>();
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();
			var order = 0;
			var pause = 0;
			var show = 0;
			var wait = 0;
			var close = 0;
			var resume = 0;

			activator1.Setup(a => a.Pause()).Callback(() => pause = ++order);
			activator1.Setup(a => a.Resume()).Callback(() => resume = ++order);
			context.Activators.Add(activator1.Object);
			context.Activators.Add(activator2.Object);
			context.Activators.Add(activator3.Object);
			lockScreen.Setup(l => l.Show()).Callback(() => show = ++order);
			lockScreen.Setup(l => l.WaitForResult()).Callback(() => wait = ++order).Returns(result);
			lockScreen.Setup(l => l.Close()).Callback(() => close = ++order);
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>()))
				.Returns(lockScreen.Object);

			sut.TryStart();
			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			activator1.Verify(a => a.Pause(), Times.Once);
			activator1.Verify(a => a.Resume(), Times.Once);
			activator2.Verify(a => a.Pause(), Times.Once);
			activator2.Verify(a => a.Resume(), Times.Once);
			activator3.Verify(a => a.Pause(), Times.Once);
			activator3.Verify(a => a.Resume(), Times.Once);
			lockScreen.Verify(l => l.Show(), Times.Once);
			lockScreen.Verify(l => l.WaitForResult(), Times.Once);
			lockScreen.Verify(l => l.Close(), Times.Once);

			Assert.IsTrue(pause == 1);
			Assert.IsTrue(show == 2);
			Assert.IsTrue(wait == 3);
			Assert.IsTrue(close == 4);
			Assert.IsTrue(resume == 5);
		}

		[TestMethod]
		public void ApplicationMonitor_MustValidateQuitPasswordIfTerminationFailed()
		{
			var hash = "12345";
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult { Password = "test" };
			var attempt = 0;
			var correct = new Random().Next(1, 50);
			var lockScreenResult = new Func<LockScreenResult>(() => ++attempt == correct ? result : new LockScreenResult());

			context.Settings.Security.QuitPasswordHash = hash;
			hashAlgorithm.Setup(a => a.GenerateHashFor(It.Is<string>(p => p == result.Password))).Returns(hash);
			lockScreen.Setup(l => l.WaitForResult()).Returns(lockScreenResult);
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>()))
				.Returns(lockScreen.Object);

			sut.TryStart();
			applicationMonitor.Raise(m => m.TerminationFailed += null, new List<RunningApplication>());

			hashAlgorithm.Verify(a => a.GenerateHashFor(It.Is<string>(p => p == result.Password)), Times.Once);
			hashAlgorithm.Verify(a => a.GenerateHashFor(It.Is<string>(p => p != result.Password)), Times.Exactly(attempt - 1));
			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.Is<IWindow>(w => w == lockScreen.Object)), Times.Exactly(attempt - 1));
		}

		[TestMethod]
		public void Browser_MustHandleSessionIdentifierDetection()
		{
			var counter = 0;
			var identifier = "abc123";

			settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.SendSessionIdentifier(It.IsAny<string>())).Returns(() => new ServerResponse(++counter == 3));

			sut.TryStart();
			browser.Raise(b => b.SessionIdentifierDetected += null, identifier);

			server.Verify(s => s.SendSessionIdentifier(It.Is<string>(id => id == identifier)), Times.Exactly(3));
		}

		[TestMethod]
		public void Browser_MustTerminateIfRequested()
		{
			runtimeProxy.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.TerminationRequested += null);

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
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

			sut.TryStart();
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

			sut.TryStart();
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

			sut.TryStart();
			clientHost.Raise(c => c.PasswordRequested += null, args);

			runtimeProxy.Verify(p => p.SubmitPassword(
				It.Is<Guid>(g => g == args.RequestId),
				It.Is<bool>(b => b == result.Success),
				It.Is<string>(s => s == result.Password)), Times.Once);
		}

		[TestMethod]
		public void Communication_MustCorrectlyHandleAbortedReconfiguration()
		{
			sut.TryStart();
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
		public void Communication_MustCorrectlyHandleServerCommunicationFailure()
		{
			var args = new ServerFailureActionRequestEventArgs { RequestId = Guid.NewGuid() };
			var dialog = new Mock<IServerFailureDialog>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ServerFailureDialogResult());
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(dialog.Object);

			sut.TryStart();
			clientHost.Raise(c => c.ServerFailureActionRequested += null, args);

			runtimeProxy.Verify(r => r.SubmitServerFailureActionResult(It.Is<Guid>(g => g == args.RequestId), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
			uiFactory.Verify(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
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
		public void DisplayMonitor_MustCorrectlyHandleDisplayChangeWithTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 25;
			var order = 0;
			var workingArea = 0;

			settings.Taskbar.EnableTaskbar = true;

			actionCenter.Setup(t => t.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			displayMonitor.Setup(m => m.InitializePrimaryDisplay(It.Is<int>(h => h == height))).Callback(() => workingArea = ++order);
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			sut.TryStart();
			displayMonitor.Raise(d => d.DisplayChanged += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Never);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Once);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Once);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(workingArea == 1);
			Assert.IsTrue(boundsActionCenter == 2);
			Assert.IsTrue(boundsTaskbar == 3);
		}

		[TestMethod]
		public void DisplayMonitor_MustCorrectlyHandleDisplayChangeWithoutTaskbar()
		{
			var boundsActionCenter = 0;
			var boundsTaskbar = 0;
			var height = 25;
			var order = 0;
			var workingArea = 0;

			settings.Taskbar.EnableTaskbar = false;

			actionCenter.Setup(t => t.InitializeBounds()).Callback(() => boundsActionCenter = ++order);
			displayMonitor.Setup(w => w.InitializePrimaryDisplay(It.Is<int>(h => h == 0))).Callback(() => workingArea = ++order);
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });
			taskbar.Setup(t => t.GetAbsoluteHeight()).Returns(height);
			taskbar.Setup(t => t.InitializeBounds()).Callback(() => boundsTaskbar = ++order);

			sut.TryStart();
			displayMonitor.Raise(d => d.DisplayChanged += null);

			actionCenter.Verify(a => a.InitializeBounds(), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == 0)), Times.Once);
			displayMonitor.Verify(d => d.InitializePrimaryDisplay(It.Is<int>(h => h == height)), Times.Never);
			taskbar.Verify(t => t.GetAbsoluteHeight(), Times.Never);
			taskbar.Verify(t => t.InitializeBounds(), Times.Once);

			Assert.IsTrue(workingArea == 1);
			Assert.IsTrue(boundsActionCenter == 2);
			Assert.IsTrue(boundsTaskbar == 3);
		}

		[TestMethod]
		public void DisplayMonitor_MustShowLockScreenOnDisplayChange()
		{
			var lockScreen = new Mock<ILockScreen>();

			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = false });
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			uiFactory.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>())).Returns(lockScreen.Object);

			sut.TryStart();
			displayMonitor.Raise(d => d.DisplayChanged += null);

			lockScreen.Verify(l => l.Show(), Times.Once);
		}

		[TestMethod]
		public void Operations_MustAskForAutomaticApplicationTermination()
		{
			var args = new ApplicationTerminationEventArgs(Enumerable.Empty<RunningApplication>());

			messageBox.Setup(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);

			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			Assert.IsTrue(args.TerminateProcesses);
		}

		[TestMethod]
		public void Operations_MustAbortAskingForAutomaticApplicationTermination()
		{
			var args = new ApplicationTerminationEventArgs(Enumerable.Empty<RunningApplication>());

			messageBox.Setup(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.No);

			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			Assert.IsFalse(args.TerminateProcesses);
		}

		[TestMethod]
		public void Operations_MustAskForApplicationPath()
		{
			var args = new ApplicationNotFoundEventArgs(default(string), default(string));
			var result = new FileSystemDialogResult { FullPath = @"C:\Some\random\path\", Success = true };

			fileSystemDialog.Setup(d => d.Show(
				It.IsAny<FileSystemElement>(),
				It.IsAny<FileSystemOperation>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<IWindow>(),
				It.IsAny<bool>(),
				It.IsAny<bool>())).Returns(result);
			text.SetReturnsDefault(string.Empty);

			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			Assert.AreEqual(result.FullPath, args.CustomPath);
			Assert.IsTrue(args.Success);
		}

		[TestMethod]
		public void Operations_MustAbortAskingForApplicationPath()
		{
			var args = new ApplicationNotFoundEventArgs(default(string), default(string));
			var result = new FileSystemDialogResult { Success = false };

			fileSystemDialog.Setup(d => d.Show(
				It.IsAny<FileSystemElement>(),
				It.IsAny<FileSystemOperation>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<IWindow>(),
				It.IsAny<bool>(),
				It.IsAny<bool>())).Returns(result);
			text.SetReturnsDefault(string.Empty);

			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			Assert.IsNull(args.CustomPath);
			Assert.IsFalse(args.Success);
		}

		[TestMethod]
		public void Operations_MustInformAboutFailedApplicationInitialization()
		{
			var args = new ApplicationInitializationFailedEventArgs(default(string), default(string), FactoryResult.NotFound);

			text.SetReturnsDefault(string.Empty);
			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Operations_MustInformAboutFailedApplicationTermination()
		{
			var args = new ApplicationTerminationFailedEventArgs(Enumerable.Empty<RunningApplication>());

			text.SetReturnsDefault(string.Empty);
			sut.TryStart();
			operationSequence.Raise(s => s.ActionRequired += null, args);

			messageBox.Verify(m => m.Show(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
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

			sut.TryStart();
			operationSequence.Raise(o => o.StatusChanged += null, key);

			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(k => k == key), It.IsAny<bool>()), Times.Once);
		}

		[TestMethod]
		public void Reconfiguration_MustAllowIfNoQuitPasswordSet()
		{
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);
			args.Callback(true, string.Empty);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustNotAllowWithQuitPasswordAndNoUrl()
		{
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			settings.Security.QuitPasswordHash = "abc123";
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustAllowIfUrlMatches()
		{
			var args = new DownloadEventArgs { Url = "sebs://www.somehost.org/some/path/some_configuration.seb?query=123" };

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			settings.Security.QuitPasswordHash = "abc123";
			settings.Security.ReconfigurationUrl = "sebs://www.somehost.org/some/path/*.seb?query=123";
			runtimeProxy.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);
			args.Callback(true, string.Empty);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustDenyIfNotAllowed()
		{
			var args = new DownloadEventArgs();

			settings.Security.AllowReconfiguration = false;
			settings.Security.QuitPasswordHash = "abc123";

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustDenyIfUrlDoesNotMatch()
		{
			var args = new DownloadEventArgs { Url = "sebs://www.somehost.org/some/path/some_configuration.seb?query=123" };

			settings.Security.AllowReconfiguration = false;
			settings.Security.QuitPasswordHash = "abc123";
			settings.Security.ReconfigurationUrl = "sebs://www.somehost.org/some/path/other_configuration.seb?query=123";

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadUrl, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath), It.Is<string>(u => u == downloadUrl)), Times.Once);

			Assert.AreEqual(downloadPath, args.DownloadPath);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(true));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(false, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedRequest()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(false));

			sut.TryStart();
			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadUrl, downloadPath);

			runtimeProxy.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void Server_MustInitiateShutdownOnEvent()
		{
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));

			sut.TryStart();
			server.Raise(s => s.TerminationRequested += null);

			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
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
		public void Shutdown_MustCloseActionCenterAndTaskbarIfEnabled()
		{
			settings.ActionCenter.EnableActionCenter = true;
			settings.Taskbar.EnableTaskbar = true;

			sut.Terminate();

			actionCenter.Verify(a => a.Close(), Times.Once);
			taskbar.Verify(o => o.Close(), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustNotCloseActionCenterAndTaskbarIfNotEnabled()
		{
			settings.ActionCenter.EnableActionCenter = false;
			settings.Taskbar.EnableTaskbar = false;

			sut.Terminate();

			actionCenter.Verify(a => a.Close(), Times.Never);
			taskbar.Verify(o => o.Close(), Times.Never);
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
			var dialogResult = new PasswordDialogResult { Password = "blobb", Success = true };

			settings.Security.QuitPasswordHash = "1234";
			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(dialogResult);
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(s => s == dialogResult.Password))).Returns(settings.Security.QuitPasswordHash);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);

			Assert.IsFalse(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustAbortAskingUserForQuitPassword()
		{
			var args = new System.ComponentModel.CancelEventArgs();
			var dialog = new Mock<IPasswordDialog>();
			var dialogResult = new PasswordDialogResult { Success = false };

			settings.Security.QuitPasswordHash = "1234";
			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(dialogResult);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			sut.TryStart();
			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Never);

			Assert.IsTrue(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustNotInitiateIfQuitPasswordIncorrect()
		{
			var args = new System.ComponentModel.CancelEventArgs();
			var dialog = new Mock<IPasswordDialog>();
			var dialogResult = new PasswordDialogResult { Password = "blobb", Success = true };

			settings.Security.QuitPasswordHash = "1234";
			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(dialogResult);
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
			context.AppConfig = null;
			context.Browser = null;
			context.ClientHost = null;
			context.IntegrityModule = null;
			context.Server = null;
			context.Settings = null;

			sut.Terminate();
		}

		[TestMethod]
		public void Startup_MustPerformOperations()
		{
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
			sut.TryStart();
			sut.UpdateAppConfig();

			splashScreen.VerifySet(s => s.AppConfig = appConfig, Times.Once);
		}

		[TestMethod]
		public void Startup_MustCorrectlyShowTaskbar()
		{
			settings.Taskbar.EnableTaskbar = true;
			sut.TryStart();

			taskbar.Verify(t => t.Show(), Times.Once);

			taskbar.Reset();
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Aborted);
			sut.TryStart();

			taskbar.Verify(t => t.Show(), Times.Never);

			taskbar.Reset();
			settings.Taskbar.EnableTaskbar = false;
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			sut.TryStart();

			taskbar.Verify(t => t.Show(), Times.Never);
		}

		[TestMethod]
		public void Startup_MustCorrectlyShowActionCenter()
		{
			settings.ActionCenter.EnableActionCenter = true;
			sut.TryStart();

			actionCenter.Verify(t => t.Promote(), Times.Once);
			actionCenter.Verify(t => t.Show(), Times.Never);

			actionCenter.Reset();
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Aborted);
			sut.TryStart();

			actionCenter.Verify(t => t.Promote(), Times.Never);
			actionCenter.Verify(t => t.Show(), Times.Never);

			actionCenter.Reset();
			settings.ActionCenter.EnableActionCenter = false;
			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			sut.TryStart();

			actionCenter.Verify(t => t.Promote(), Times.Never);
			actionCenter.Verify(t => t.Show(), Times.Never);
		}

		[TestMethod]
		public void Startup_MustAutoStartApplications()
		{
			var application1 = new Mock<IApplication>();
			var application2 = new Mock<IApplication>();
			var application3 = new Mock<IApplication>();

			application1.SetupGet(a => a.AutoStart).Returns(true);
			application2.SetupGet(a => a.AutoStart).Returns(false);
			application3.SetupGet(a => a.AutoStart).Returns(true);
			context.Applications.Add(application1.Object);
			context.Applications.Add(application2.Object);
			context.Applications.Add(application3.Object);

			sut.TryStart();

			application1.Verify(a => a.Start(), Times.Once);
			application2.Verify(a => a.Start(), Times.Never);
			application3.Verify(a => a.Start(), Times.Once);
		}

		[TestMethod]
		public void Startup_MustAutoStartBrowser()
		{
			settings.Browser.EnableBrowser = true;
			browser.SetupGet(b => b.AutoStart).Returns(true);

			sut.TryStart();

			browser.Verify(b => b.Start(), Times.Once);
			browser.Reset();
			browser.SetupGet(b => b.AutoStart).Returns(false);

			sut.TryStart();

			browser.Verify(b => b.Start(), Times.Never);
		}

		[TestMethod]
		public void Startup_MustNotAutoStartBrowserIfNotEnabled()
		{
			settings.Browser.EnableBrowser = false;
			browser.SetupGet(b => b.AutoStart).Returns(true);

			sut.TryStart();

			browser.Verify(b => b.Start(), Times.Never);
		}

		[TestMethod]
		public void SystemMonitor_MustShowLockScreenOnSessionSwitch()
		{
			var lockScreen = new Mock<ILockScreen>();

			settings.Service.IgnoreService = true;
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			uiFactory.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>())).Returns(lockScreen.Object);

			sut.TryStart();
			systemMonitor.Raise(m => m.SessionSwitched += null);

			lockScreen.Verify(l => l.Show(), Times.Once);
		}

		[TestMethod]
		public void SystemMonitor_MustTerminateIfRequestedByUser()
		{
			var lockScreen = new Mock<ILockScreen>();
			var result = new LockScreenResult();

			settings.Service.IgnoreService = true;
			lockScreen.Setup(l => l.WaitForResult()).Returns(result);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory
				.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>()))
				.Callback(new Action<string, string, IEnumerable<LockScreenOption>>((message, title, options) => result.OptionId = options.Last().Id))
				.Returns(lockScreen.Object);

			sut.TryStart();
			systemMonitor.Raise(m => m.SessionSwitched += null);

			lockScreen.Verify(l => l.Show(), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void SystemMonitor_MustDoNothingIfSessionSwitchAllowed()
		{
			var lockScreen = new Mock<ILockScreen>();

			settings.Service.IgnoreService = false;
			settings.Service.DisableUserLock = false;
			settings.Service.DisableUserSwitch = false;
			lockScreen.Setup(l => l.WaitForResult()).Returns(new LockScreenResult());
			uiFactory.Setup(f => f.CreateLockScreen(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LockScreenOption>>())).Returns(lockScreen.Object);

			sut.TryStart();
			systemMonitor.Raise(m => m.SessionSwitched += null);

			lockScreen.Verify(l => l.Show(), Times.Never);
		}

		[TestMethod]
		public void TerminationActivator_MustCorrectlyInitiateShutdown()
		{
			var order = 0;
			var pause = 0;
			var resume = 0;
			var terminationActivator = new Mock<ITerminationActivator>();

			context.Activators.Add(terminationActivator.Object);
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtimeProxy.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			terminationActivator.Setup(t => t.Pause()).Callback(() => pause = ++order);
			terminationActivator.Setup(t => t.Resume()).Callback(() => resume = ++order);

			sut.TryStart();
			terminationActivator.Raise(t => t.Activated += null);

			Assert.AreEqual(1, pause);
			Assert.AreEqual(2, resume);
			terminationActivator.Verify(t => t.Pause(), Times.Once);
			terminationActivator.Verify(t => t.Resume(), Times.Once);
			runtimeProxy.Verify(p => p.RequestShutdown(), Times.Once);
		}
	}
}
