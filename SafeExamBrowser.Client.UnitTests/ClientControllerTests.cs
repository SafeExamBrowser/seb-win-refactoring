/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using IWindow = SafeExamBrowser.UserInterface.Contracts.Windows.IWindow;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class ClientControllerTests
	{
		private AppConfig appConfig;
		private ClientContext context;
		private Mock<IFileSystemDialog> fileSystemDialog;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IOperationSequence> operationSequence;
		private Mock<IResponsibilityCollection<ClientTask>> responsibilities;
		private Mock<IRuntimeProxy> runtimeProxy;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;

		private ClientController sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			context = new ClientContext();
			fileSystemDialog = new Mock<IFileSystemDialog>();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			operationSequence = new Mock<IOperationSequence>();
			responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();
			runtimeProxy = new Mock<IRuntimeProxy>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();

			operationSequence.Setup(o => o.TryPerform()).Returns(OperationResult.Success);
			runtimeProxy.Setup(r => r.InformClientReady()).Returns(new CommunicationResult(true));

			sut = new ClientController(
				context,
				fileSystemDialog.Object,
				logger.Object,
				messageBox.Object,
				operationSequence.Object,
				responsibilities.Object,
				runtimeProxy.Object,
				splashScreen.Object,
				text.Object);

			context.AppConfig = appConfig;
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
			var args = new ApplicationNotFoundEventArgs(default, default);
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
			var args = new ApplicationNotFoundEventArgs(default, default);
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
			var args = new ApplicationInitializationFailedEventArgs(default, default, FactoryResult.NotFound);

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
			var key = TextKey.OperationStatus_InitializeClipboard;

			sut.TryStart();
			operationSequence.Raise(o => o.StatusChanged += null, key);

			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(k => k == key), It.IsAny<bool>()), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustRevertOperations()
		{
			operationSequence.Setup(o => o.TryRevert()).Returns(OperationResult.Success);
			sut.Terminate();
			operationSequence.Verify(o => o.TryRevert(), Times.Once);
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
			// sut.TryStart();
			sut.UpdateAppConfig();

			splashScreen.VerifySet(s => s.AppConfig = appConfig, Times.Once);
		}
	}
}
