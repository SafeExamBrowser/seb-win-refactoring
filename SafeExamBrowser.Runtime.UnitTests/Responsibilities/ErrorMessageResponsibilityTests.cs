/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class ErrorMessageResponsibilityTests
	{
		private AppConfig appConfig;
		private Mock<IErrorDialog> errorDialog;
		private Mock<ILogger> logger;
		private Mock<IMailClient> mailClient;
		private RuntimeContext context;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ErrorMessageResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig
			{
				BrowserLogFilePath = @"C:\Logs\Browser.log",
				ClientLogFilePath = @"C:\Logs\Client.log",
				RuntimeLogFilePath = @"C:\Logs\Runtime.log",
				ServiceLogFilePath = @"C:\Logs\Service.log"
			};
			context = new RuntimeContext();
			errorDialog = new Mock<IErrorDialog>();
			logger = new Mock<ILogger>();
			mailClient = new Mock<IMailClient>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns<TextKey>(key => key.ToString());
			uiFactory.Setup(f => f.CreateErrorDialog(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<Action>(), It.IsAny<bool>(), It.IsAny<string[]>())).Returns(errorDialog.Object);

			sut = new ErrorMessageResponsibility(appConfig, logger.Object, mailClient.Object, context, runtimeWindow.Object, splashScreen.Object, text.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustShowErrorDialogForApplicationCrash()
		{
			sut.TryAssume<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, Array.Empty<string>(), out _);

			errorDialog.Verify(d => d.Show(It.Is<IWindow>(parent => parent == splashScreen.Object)));
			uiFactory.Verify(f => f.CreateErrorDialog(
				It.Is<TextKey>(m => m == TextKey.ErrorDialog_CrashMessage),
				It.Is<TextKey>(t => t == TextKey.ErrorDialog_CrashTitle),
				It.IsAny<Action>(),
				It.IsAny<bool>(),
				It.IsAny<string[]>()), Times.Once);
		}

		[TestMethod]
		public void MustShowErrorDialogForSessionStartError()
		{
			sut.Assume(RuntimeTask.ShowSessionStartError);

			errorDialog.Verify(d => d.Show(It.Is<IWindow>(parent => parent == runtimeWindow.Object)));
			uiFactory.Verify(f => f.CreateErrorDialog(
				It.Is<TextKey>(m => m == TextKey.ErrorDialog_SessionStartMessage),
				It.Is<TextKey>(t => t == TextKey.ErrorDialog_SessionStartTitle),
				It.IsAny<Action>(),
				It.IsAny<bool>(),
				It.IsAny<string[]>()), Times.Once);
		}

		[TestMethod]
		public void MustShowErrorDialogForShutdownError()
		{
			sut.Assume(RuntimeTask.ShowShutdownError);

			errorDialog.Verify(d => d.Show(It.Is<IWindow>(parent => parent == splashScreen.Object)));
			uiFactory.Verify(f => f.CreateErrorDialog(
				It.Is<TextKey>(m => m == TextKey.ErrorDialog_ShutdownMessage),
				It.Is<TextKey>(t => t == TextKey.ErrorDialog_ShutdownTitle),
				It.IsAny<Action>(),
				It.IsAny<bool>(),
				It.IsAny<string[]>()), Times.Once);
		}

		[TestMethod]
		public void MustShowErrorDialogForStartupError()
		{
			sut.Assume(RuntimeTask.ShowStartupError);

			errorDialog.Verify(d => d.Show(It.Is<IWindow>(parent => parent == splashScreen.Object)));
			uiFactory.Verify(f => f.CreateErrorDialog(
				It.Is<TextKey>(m => m == TextKey.ErrorDialog_StartupMessage),
				It.Is<TextKey>(t => t == TextKey.ErrorDialog_StartupTitle),
				It.IsAny<Action>(),
				It.IsAny<bool>(),
				It.IsAny<string[]>()), Times.Once);
		}
	}
}