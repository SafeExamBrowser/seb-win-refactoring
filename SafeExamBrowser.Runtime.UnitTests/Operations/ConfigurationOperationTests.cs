/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;

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
		private SessionConfiguration nextSession;
		private SessionContext sessionContext;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			fileSystem = new Mock<IFileSystem>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			logger = new Mock<ILogger>();
			repository = new Mock<IConfigurationRepository>();
			currentSession = new SessionConfiguration();
			nextSession = new SessionConfiguration();
			sessionContext = new SessionContext();

			appConfig.AppDataFilePath = $@"C:\Not\Really\AppData\File.xml";
			appConfig.ProgramDataFilePath = $@"C:\Not\Really\ProgramData\File.xml";
			currentSession.AppConfig = appConfig;
			nextSession.AppConfig = appConfig;
			sessionContext.Current = currentSession;
			sessionContext.Next = nextSession;
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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			sessionContext.Current = null;
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is ConfigurationCompletedEventArgs c)
				{
					c.AbortStartup = true;
				}
			};

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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is ConfigurationCompletedEventArgs c)
				{
					c.AbortStartup = false;
				}
			};

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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is ClientConfigurationErrorMessageArgs)
				{
					informed = true;
				}
			};

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

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is ConfigurationCompletedEventArgs c)
				{
					Assert.Fail();
				}
			};

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotFailWithoutCommandLineArgs()
		{
			var defaultSettings = new AppSettings();
			var result = OperationResult.Failed;

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);

			sut = new ConfigurationOperation(new string[] { }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);
		}

		[TestMethod]
		public void Perform_MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";
			var sut = new ConfigurationOperation(new[] { "blubb.exe", uri }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var count = 0;
			var localSettings = new AppSettings();
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			localSettings.Security.AdminPasswordHash = "1234";
			settings.Security.AdminPasswordHash = "9876";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.LocalPath.Contains(FILE_NAME)), out localSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			nextSession.Settings = settings;

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p && p.Purpose == PasswordRequestPurpose.LocalAdministrator)
				{
					count++;
					p.Success = true;
				}
			};

			var result = sut.Perform();

			Assert.AreEqual(5, count);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var count = 0;
			var settings = default(AppSettings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p && p.Purpose == PasswordRequestPurpose.Settings)
				{
					count++;
					p.Success = true;
				}
			};

			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>()), Times.Exactly(6));

			Assert.AreEqual(5, count);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfAdminPasswordCorrect()
		{
			var password = "test";
			var currentSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			currentSettings.Security.AdminPasswordHash = "1234";
			nextSession.Settings = nextSettings;
			nextSettings.Security.AdminPasswordHash = "9876";
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.Security.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p && p.Purpose == PasswordRequestPurpose.LocalAdministrator)
				{
					p.Password = password;
					p.Success = true;
				}
			};

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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs)
				{
					Assert.Fail();
				}
			};

			var result = sut.Perform();

			hashAlgorithm.Verify(h => h.GenerateHashFor(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfSettingsPasswordCorrect()
		{
			var password = "test";
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == password))).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = password;
					p.Success = true;
				}
			};

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

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == settings.Security.AdminPasswordHash)), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var password = "test";
			var currentSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			currentSettings.Security.AdminPasswordHash = "1234";
			nextSession.Settings = nextSettings;
			nextSettings.Security.AdminPasswordHash = "9876";

			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.Security.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p && p.Purpose == PasswordRequestPurpose.LocalAdministrator)
				{
					p.Success = false;
				}
			};

			var result = sut.Perform();

			repository.Verify(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustAbortAskingForSettingsPasswordIfDecidedByUser()
		{
			var settings = default(AppSettings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = false;
				}
			};

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
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => delete = ++order);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success).Callback(() => configure = ++order);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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

			sessionContext.ReconfigurationFilePath = null;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Never);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);

			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Never);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustAbortForSettingsPasswordIfWishedByUser()
		{
			var currentSettings = new AppSettings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new AppSettings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.Settings = currentSettings;
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = false;
				}
			};

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.Is<string>(s => s == resource.LocalPath)), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var sut = new ConfigurationOperation(null, repository.Object, fileSystem.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Revert();

			fileSystem.VerifyNoOtherCalls();
			hashAlgorithm.VerifyNoOtherCalls();
			repository.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
