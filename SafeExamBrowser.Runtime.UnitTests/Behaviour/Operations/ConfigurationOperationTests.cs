/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IPasswordDialog> passwordDialog;
		private Mock<IConfigurationRepository> repository;
		private Mock<IResourceLoader> resourceLoader;
		private Mock<IRuntimeHost> runtimeHost;
		private Settings settings;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			passwordDialog = new Mock<IPasswordDialog>();
			repository = new Mock<IConfigurationRepository>();
			resourceLoader = new Mock<IResourceLoader>();
			runtimeHost = new Mock<IRuntimeHost>();
			settings = new Settings();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			appConfig.AppDataFolder = @"C:\Not\Really\AppData";
			appConfig.DefaultSettingsFileName = "SettingsDummy.txt";
			appConfig.ProgramDataFolder = @"C:\Not\Really\ProgramData";

			repository.SetupGet(r => r.CurrentSettings).Returns(settings);

			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordDialog.Object);
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = location;

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			var resource = new Uri(url);

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = $@"{location}\WRONG";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			appConfig.AppDataFolder = location;

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			appConfig.ProgramDataFolder = Path.GetDirectoryName(GetType().Assembly.Location);
			messageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.Yes);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustNotAbortIfNotWishedByUser()
		{
			messageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.No);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustNotAllowToAbortIfNotInConfigureClientMode()
		{
			settings.ConfigurationMode = ConfigurationMode.Exam;
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);
			sut.Perform();

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>()), Times.Never);
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			repository.Setup(r => r.LoadDefaultSettings());

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);
			sut.Perform();

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new string[] { });
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", uri });
			sut.Perform();
		}

		[TestMethod]
		public void MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var result = new PasswordDialogResultStub { Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var result = new PasswordDialogResultStub { Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustSucceedIfAdminPasswordCorrect()
		{
			var password = "test";
			var result = new PasswordDialogResultStub { Password = password, Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), password, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), password, null), Times.Once);
		}

		[TestMethod]
		public void MustSucceedIfSettingsPasswordCorrect()
		{
			var password = "test";
			var result = new PasswordDialogResultStub { Password = password, Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, password)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, password), Times.Once);
		}

		[TestMethod]
		public void MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var dialogResult = new PasswordDialogResultStub { Success = false };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(dialogResult);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustAbortAskingForSettingsPasswordIfDecidedByUser()
		{
			var dialogResult = new PasswordDialogResultStub { Success = false };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(dialogResult);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustAllowEnteringBothPasswords()
		{
			var adminPassword = "xyz";
			var adminResult = new PasswordDialogResultStub { Password = adminPassword, Success = true };
			var adminCallback = new Action(() => passwordDialog.Setup(d => d.Show(null)).Returns(adminResult));
			var settingsPassword = "abc";
			var settingsResult = new PasswordDialogResultStub { Password = settingsPassword, Success = true };
			var settingsCallback = new Action(() => passwordDialog.Setup(d => d.Show(null)).Returns(settingsResult));
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded).Callback(settingsCallback);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, settingsPassword)).Returns(LoadStatus.AdminPasswordNeeded).Callback(adminCallback);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), adminPassword, settingsPassword)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, settingsPassword), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), adminPassword, settingsPassword), Times.Once);
		}

		[TestMethod]
		public void MustRequestPasswordViaDialogOnDefaultDesktop()
		{
			var clientProxy = new Mock<IClientProxy>();
			var session = new Mock<ISessionData>();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(new PasswordDialogResultStub { Success = true });
			repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
			settings.KioskMode = KioskMode.DisableExplorerShell;

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Never);
			passwordDialog.Verify(p => p.Show(null), Times.AtLeastOnce);
			session.VerifyGet(s => s.ClientProxy, Times.Never);
		}

		[TestMethod]
		public void MustRequestPasswordViaClientDuringReconfigurationOnNewDesktop()
		{
			var clientProxy = new Mock<IClientProxy>();
			var communication = new CommunicationResult(true);
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = true });
			});
			var session = new Mock<ISessionData>();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication).Callback(passwordReceived);
			passwordDialog.Setup(d => d.Show(null)).Returns(new PasswordDialogResultStub { Success = true });
			repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
			settings.KioskMode = KioskMode.CreateNewDesktop;

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.AtLeastOnce);
			passwordDialog.Verify(p => p.Show(null), Times.Never);
			session.VerifyGet(s => s.ClientProxy, Times.AtLeastOnce);
		}

		[TestMethod]
		public void MustAbortAskingForPasswordViaClientIfDecidedByUser()
		{
			var clientProxy = new Mock<IClientProxy>();
			var communication = new CommunicationResult(true);
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = false });
			});
			var session = new Mock<ISessionData>();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication).Callback(passwordReceived);
			repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
			settings.KioskMode = KioskMode.CreateNewDesktop;

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustNotWaitForPasswordViaClientIfCommunicationHasFailed()
		{
			var clientProxy = new Mock<IClientProxy>();
			var communication = new CommunicationResult(false);
			var session = new Mock<ISessionData>();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication);
			repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
			settings.KioskMode = KioskMode.CreateNewDesktop;

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustHandleInvalidData()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			resourceLoader.Setup(r => r.IsHtmlResource(It.IsAny<Uri>())).Returns(false);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.InvalidData);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustHandleHtmlAsInvalidData()
		{
			var url = "http://www.blubb.org/some/resource.html";

			resourceLoader.Setup(r => r.IsHtmlResource(It.IsAny<Uri>())).Returns(true);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.InvalidData);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(url, settings.Browser.StartUrl);
		}

		[TestMethod]
		public void MustReconfigureSuccessfullyWithCorrectUri()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.SetupGet(r => r.ReconfigurationFilePath).Returns(resource.AbsolutePath);
			repository.Setup(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);

			var result = sut.Repeat();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustFailToReconfigureWithInvalidUri()
		{
			var resource = new Uri("file:///C:/does/not/exist.txt");

			repository.SetupGet(r => r.ReconfigurationFilePath).Returns(null as string);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, messageBox.Object, resourceLoader.Object, runtimeHost.Object, text.Object, uiFactory.Object, null);

			var result = sut.Repeat();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);

			repository.SetupGet(r => r.ReconfigurationFilePath).Returns(resource.AbsolutePath);
			result = sut.Repeat();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);
		}
	}
}
