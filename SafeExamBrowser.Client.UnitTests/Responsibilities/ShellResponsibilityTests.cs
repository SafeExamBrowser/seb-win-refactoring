/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class ShellResponsibilityTests
	{
		private Mock<IActionCenter> actionCenter;
		private ClientContext context;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<IMessageBox> messageBox;
		private Mock<IRuntimeProxy> runtime;
		private AppSettings settings;
		private Mock<ITaskbar> taskbar;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ShellResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();
			var responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();

			actionCenter = new Mock<IActionCenter>();
			context = new ClientContext();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			messageBox = new Mock<IMessageBox>();
			runtime = new Mock<IRuntimeProxy>();
			settings = new AppSettings();
			taskbar = new Mock<ITaskbar>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.MessageBox = messageBox.Object;
			context.Responsibilities = responsibilities.Object;
			context.Runtime = runtime.Object;
			context.Settings = settings;

			sut = new ShellResponsibility(
				actionCenter.Object,
				context,
				hashAlgorithm.Object,
				logger.Object,
				messageBox.Object,
				taskbar.Object,
				uiFactory.Object);

			sut.Assume(ClientTask.RegisterEvents);
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
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			runtime.Verify(p => p.RequestShutdown(), Times.Once);
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

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			runtime.Verify(p => p.RequestShutdown(), Times.Never);
			Assert.IsTrue(args.Cancel);
		}

		[TestMethod]
		public void Shutdown_MustCloseActionCenterAndTaskbarIfEnabled()
		{
			settings.UserInterface.ActionCenter.EnableActionCenter = true;
			settings.UserInterface.Taskbar.EnableTaskbar = true;

			sut.Assume(ClientTask.CloseShell);

			actionCenter.Verify(a => a.Close(), Times.Once);
			taskbar.Verify(o => o.Close(), Times.Once);
		}

		[TestMethod]
		public void Shutdown_MustNotCloseActionCenterAndTaskbarIfNotEnabled()
		{
			settings.UserInterface.ActionCenter.EnableActionCenter = false;
			settings.UserInterface.Taskbar.EnableTaskbar = false;

			sut.Assume(ClientTask.CloseShell);

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
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(false));

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Once);
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
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Once);

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
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			uiFactory.Setup(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>())).Returns(dialog.Object);

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Never);

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

			taskbar.Raise(t => t.QuitButtonClicked += null, args as object);

			messageBox.Verify(m => m.Show(
			It.IsAny<TextKey>(),
			It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Warning),
				It.IsAny<IWindow>()), Times.Once);
			uiFactory.Verify(u => u.CreatePasswordDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>()), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Never);

			Assert.IsTrue(args.Cancel);
		}

		[TestMethod]
		public void Startup_MustCorrectlyShowTaskbar()
		{
			settings.UserInterface.Taskbar.EnableTaskbar = true;
			sut.Assume(ClientTask.ShowShell);
			taskbar.Verify(t => t.Show(), Times.Once);

			taskbar.Reset();
			settings.UserInterface.Taskbar.EnableTaskbar = false;
			sut.Assume(ClientTask.ShowShell);

			taskbar.Verify(t => t.Show(), Times.Never);
		}

		[TestMethod]
		public void Startup_MustCorrectlyShowActionCenter()
		{
			settings.UserInterface.ActionCenter.EnableActionCenter = true;
			sut.Assume(ClientTask.ShowShell);

			actionCenter.Verify(t => t.Promote(), Times.Once);
			actionCenter.Verify(t => t.Show(), Times.Never);

			actionCenter.Reset();
			settings.UserInterface.ActionCenter.EnableActionCenter = false;
			sut.Assume(ClientTask.ShowShell);

			actionCenter.Verify(t => t.Promote(), Times.Never);
			actionCenter.Verify(t => t.Show(), Times.Never);
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
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));
			terminationActivator.Setup(t => t.Pause()).Callback(() => pause = ++order);
			terminationActivator.Setup(t => t.Resume()).Callback(() => resume = ++order);

			sut.Assume(ClientTask.RegisterEvents);
			terminationActivator.Raise(t => t.Activated += null);

			Assert.AreEqual(1, pause);
			Assert.AreEqual(2, resume);
			terminationActivator.Verify(t => t.Pause(), Times.Once);
			terminationActivator.Verify(t => t.Resume(), Times.Once);
			runtime.Verify(p => p.RequestShutdown(), Times.Once);
		}
	}
}
