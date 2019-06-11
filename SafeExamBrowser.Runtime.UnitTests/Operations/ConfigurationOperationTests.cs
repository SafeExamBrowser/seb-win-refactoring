/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private const string FILE_NAME = "SebClientSettings.seb";

		private AppConfig appConfig;
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
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);

			appConfig.AppDataFilePath = location;
			appConfig.ProgramDataFilePath = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();
			var resource = new Uri(url);

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustUseProgramDataAs2ndPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			var settings = default(Settings);

			appConfig.ProgramDataFilePath = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(location)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustUseAppDataAs3rdPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			var settings = default(Settings);

			appConfig.AppDataFilePath = location;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(location)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustTestdatalyHandleBrowserResource()
		{
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			nextSession.Settings = settings;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.LoadWithBrowser);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			Assert.AreEqual(url, settings.Browser.StartUrl);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFallbackToDefaultsAsLastPrio()
		{
			var defaultSettings = new Settings();

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);
		}

		[TestMethod]
		public void Perform_MustAbortIfWishedByUser()
		{
			var settings = new Settings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			sessionContext.Current = null;
			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = new Settings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = new Settings();
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			settings.ConfigurationMode = ConfigurationMode.ConfigureClient;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.UnexpectedError);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = new Settings();

			settings.ConfigurationMode = ConfigurationMode.Exam;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var defaultSettings = new Settings();
			var result = OperationResult.Failed;

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);

			sut = new ConfigurationOperation(new string[] { }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, nextSession.Settings);
		}

		[TestMethod]
		public void Perform_MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";
			var sut = new ConfigurationOperation(new[] { "blubb.exe", uri }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var count = 0;
			var localSettings = new Settings { AdminPasswordHash = "1234" };
			var settings = new Settings { AdminPasswordHash = "9876", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.LocalPath.Contains(FILE_NAME)), out localSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			nextSession.Settings = settings;

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = default(Settings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
		public void Perform_MustSucceedIfAdminPasswordTestdata()
		{
			var password = "test";
			var currentSettings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new Settings { AdminPasswordHash = "9876", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			nextSession.Settings = nextSettings;
			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var currentSettings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			nextSession.Settings = nextSettings;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.IsAny<Uri>(), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
		public void Perform_MustSucceedIfSettingsPasswordTestdata()
		{
			var password = "test";
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == password))).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.Exam };

			appConfig.AppDataFilePath = location;

			repository
				.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.PasswordNeeded);
			repository
				.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(new Uri(location))), out settings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			repository
				.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.IsHash == true && p.Password == settings.AdminPasswordHash)))
				.Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.Is<PasswordParameters>(p => p.Password == settings.AdminPasswordHash)), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var password = "test";
			var currentSettings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new Settings { AdminPasswordHash = "9876", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations), "Testdata", FILE_NAME);
			nextSession.Settings = nextSettings;

			hashAlgorithm.Setup(h => h.GenerateHashFor(It.Is<string>(p => p == password))).Returns(currentSettings.AdminPasswordHash);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out currentSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.AbsoluteUri == url), out nextSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
			var settings = default(Settings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
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
		public void Repeat_MustPerformForExamWithTestdataUri()
		{
			var currentSettings = new Settings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };

			currentSession.Settings = currentSettings;
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustPerformForClientConfigurationWithTestdataUri()
		{
			var currentSettings = new Settings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new Settings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.Settings = currentSettings;
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustFailWithInvalidUri()
		{
			var resource = new Uri("file:///C:/does/not/exist.txt");
			var settings = default(Settings);

			sessionContext.ReconfigurationFilePath = null;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);

			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustAbortForSettingsPasswordIfWishedByUser()
		{
			var currentSettings = new Settings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "Testdata", FILE_NAME));
			var settings = new Settings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.Settings = currentSettings;
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.PasswordNeeded);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = false;
				}
			};

			var result = sut.Repeat();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Revert();

			hashAlgorithm.VerifyNoOtherCalls();
			repository.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
