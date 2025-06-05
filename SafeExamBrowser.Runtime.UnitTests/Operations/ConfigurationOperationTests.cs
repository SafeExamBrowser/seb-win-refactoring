/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private const string FILE_NAME = "SebClientSettings.seb";

		private AppConfig appConfig;
		private Mock<IFileSystem> fileSystem;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<ILogger> logger;
		private Mock<IConfigurationRepository> repository;
		private SessionConfiguration currentSession;
		private Mock<IMessageBox> messageBox;
		private SessionConfiguration nextSession;
		private RuntimeContext runtimeContext;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ClientBridge clientBridge;
		private Dependencies dependencies;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			fileSystem = new Mock<IFileSystem>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			logger = new Mock<ILogger>();
			repository = new Mock<IConfigurationRepository>();
			currentSession = new SessionConfiguration();
			messageBox = new Mock<IMessageBox>();
			nextSession = new SessionConfiguration();
			runtimeContext = new RuntimeContext();
			runtimeHost = new Mock<IRuntimeHost>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			appConfig.AppDataFilePath = $@"C:\Not\Really\AppData\File.xml";
			appConfig.ProgramDataFilePath = $@"C:\Not\Really\ProgramData\File.xml";
			clientBridge = new ClientBridge(runtimeHost.Object, runtimeContext);
			currentSession.AppConfig = appConfig;
			dependencies = new Dependencies(clientBridge, logger.Object, messageBox.Object, runtimeWindow.Object, runtimeContext, text.Object);
			nextSession.AppConfig = appConfig;
			runtimeContext.Current = currentSession;
			runtimeContext.Next = nextSession;
		}

		[TestMethod]
		public void Perform_MustUseCommandLineArgumentAs1stPrio()
		{
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);

			appConfig.AppDataFilePath = location;
			appConfig.ProgramDataFilePath = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();
			var resource = new Uri(url);

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustUseProgramDataAs2ndPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			var settings = default(AppSettings);

			appConfig.ProgramDataFilePath = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(location)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustUseAppDataAs3rdPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			var settings = default(AppSettings);

			appConfig.AppDataFilePath = location;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(location)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustCorrectlyHandleBrowserResource()
		{
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			nextSession.Settings = settings;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.LoadWithBrowser);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.IsFalse(settings.Browser.DeleteCacheOnShutdown);
			Assert.IsFalse(settings.Browser.DeleteCookiesOnShutdown);
			Assert.IsTrue(settings.Security.AllowReconfiguration);
			Assert.AreEqual(url, settings.Browser.StartUrl);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFallbackToDefaultsAsLastPrio()
		{
			var defaultSettings = new AppSettings();

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);
		}

		[TestMethod]
		public void Perform_MustAbortIfWishedByUser()
		{
			var settings = new AppSettings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			runtimeContext.Current = null;
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			messageBox
				.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Returns(MessageBoxResult.Yes);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustNotAbortIfNotWishedByUser()
		{
			var settings = new AppSettings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			messageBox
				.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Returns(MessageBoxResult.No);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustInformAboutClientConfigurationError()
		{
			var informed = false;
			var settings = new AppSettings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.UnexpectedError);

			messageBox
				.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => informed = true);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(informed);
		}

		[TestMethod]
		public void Perform_MustNotAllowToAbortIfNotInConfigureClientMode()
		{
			var settings = new AppSettings();

			settings.ConfigurationMode = ConfigurationMode.Exam;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			messageBox
				.Setup(m => m.Show(It.IsAny<TextKey>(), It.IsAny<TextKey>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(Assert.Fail);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotFailWithoutCommandLineArgs()
		{
			var defaultSettings = new AppSettings();
			var result = OperationResult.Failed;

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);

			sut = new ConfigurationOperation(new string[] { }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);
		}

		[TestMethod]
		public void Perform_MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";
			var sut = new ConfigurationOperation(new[] { "abc.exe", uri }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var count = 0;
			var dialog = new Mock<IPasswordDialog>();
			var localSettings = new AppSettings();
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			localSettings.Security.AdminPasswordHash = "1234";
			settings.Security.AdminPasswordHash = "9876";
			nextSession.Settings = settings;

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(() => count++).Returns(new PasswordDialogResult { Success = true });
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.LocalPath.Contains(FILE_NAME)), out localSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(5, count);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var count = 0;
			var dialog = new Mock<IPasswordDialog>();
			var settings = default(AppSettings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(() => count++).Returns(new PasswordDialogResult { Success = true });
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>()), Times.Exactly(6));

			Assert.AreEqual(5, count);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfAdminPasswordCorrect()
		{
			var dialog = new Mock<IPasswordDialog>();
			var password = "test";
			var currentSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			currentSettings.Security.AdminPasswordHash = "1234";
			nextSession.Settings = nextSettings;
			nextSettings.Security.AdminPasswordHash = "9876";

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new PasswordDialogResult { Password = password, Success = true });
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.Security.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotAuthenticateIfSameAdminPassword()
		{
			var currentSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			currentSettings.Security.AdminPasswordHash = "1234";
			nextSession.Settings = nextSettings;
			nextSettings.Security.AdminPasswordHash = "1234";
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Callback(Assert.Fail);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			hashAlgorithm.Verify(h => h.GenerateHashFor(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfSettingsPasswordCorrect()
		{
			var dialog = new Mock<IPasswordDialog>();
			var password = "test";
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new PasswordDialogResult { Password = password, Success = true });
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == password))).Returns(LoadStatus.Success);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == password)), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustUseCurrentPasswordIfAvailable()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };

			appConfig.AppDataFilePath = location;
			settings.Security.AdminPasswordHash = "1234";

			repository
				.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.PasswordNeeded);
			repository
				.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(new Uri(location))), out settings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			repository
				.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.IsHash == true && p.Password == settings.Security.AdminPasswordHash)))
				.Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == settings.Security.AdminPasswordHash)), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var dialog = new Mock<IPasswordDialog>();
			var password = "test";
			var currentSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			currentSettings.Security.AdminPasswordHash = "1234";
			nextSession.Settings = nextSettings;
			nextSettings.Security.AdminPasswordHash = "9876";

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new PasswordDialogResult { Success = false });
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.Security.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			repository.Verify(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustAbortAskingForSettingsPasswordIfDecidedByUser()
		{
			var dialog = new Mock<IPasswordDialog>();
			var settings = default(AppSettings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new PasswordDialogResult { Success = false });
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(new[] { "abc.exe", url }, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustPerformForExamWithCorrectUri()
		{
			var currentSettings = new AppSettings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };

			currentSession.Settings = currentSettings;
			runtimeContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustPerformForClientConfigurationWithCorrectUri()
		{
			var currentSettings = new AppSettings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.Settings = currentSettings;
			runtimeContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustDeleteTemporaryFileAfterClientConfiguration()
		{
			var currentSettings = new AppSettings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var delete = 0;
			var configure = 0;
			var order = 0;

			currentSession.Settings = currentSettings;
			runtimeContext.ReconfigurationFilePath = resource.LocalPath;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => delete = ++order);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success).Callback(() => configure = ++order);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(1, configure);
			Assert.AreEqual(2, delete);
		}

		[TestMethod]
		public void Repeat_MustFailWithInvalidUri()
		{
			var resource = new Uri("file:///C:/does/not/exist.txt");
			var settings = default(AppSettings);

			runtimeContext.ReconfigurationFilePath = null;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Never);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);

			runtimeContext.ReconfigurationFilePath = resource.LocalPath;
			result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Never);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustAbortForSettingsPasswordIfWishedByUser()
		{
			var currentSettings = new AppSettings();
			var dialog = new Mock<IPasswordDialog>();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.Settings = currentSettings;
			runtimeContext.ReconfigurationFilePath = resource.LocalPath;

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new PasswordDialogResult { Success = true });
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(dialog.Object);

			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var sut = new ConfigurationOperation(null, dependencies, fileSystem.Object, hashAlgorithm.Object, repository.Object, uiFactory.Object);
			var result = sut.Revert();

			fileSystem.VerifyNoOtherCalls();
			hashAlgorithm.VerifyNoOtherCalls();
			repository.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
