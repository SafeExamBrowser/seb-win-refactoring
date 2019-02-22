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
		private AppConfig appConfig;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<ILogger> logger;
		private Mock<IConfigurationRepository> repository;
		private Mock<ISessionConfiguration> currentSession;
		private Mock<ISessionConfiguration> nextSession;
		private SessionContext sessionContext;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			logger = new Mock<ILogger>();
			repository = new Mock<IConfigurationRepository>();
			currentSession = new Mock<ISessionConfiguration>();
			nextSession = new Mock<ISessionConfiguration>();
			sessionContext = new SessionContext();

			appConfig.AppDataFolder = @"C:\Not\Really\AppData";
			appConfig.DefaultSettingsFileName = "SettingsDummy.txt";
			appConfig.ProgramDataFolder = @"C:\Not\Really\ProgramData";
			currentSession.SetupGet(s => s.AppConfig).Returns(appConfig);
			nextSession.SetupGet(s => s.AppConfig).Returns(appConfig);
			sessionContext.Current = currentSession.Object;
			sessionContext.Next = nextSession.Object;
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.AppDataFolder = location;
			appConfig.ProgramDataFolder = location;
			appConfig.DefaultSettingsFileName = "SettingsDummy.txt";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();
			var resource = new Uri(url);

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));
			var settings = default(Settings);

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = $@"{location}\WRONG";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();
			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));
			var settings = default(Settings);

			appConfig.AppDataFolder = location;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();
			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			var actualSettings = default(Settings);
			var defaultSettings = new Settings();

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);
			nextSession.SetupSet<Settings>(s => s.Settings = It.IsAny<Settings>()).Callback(s => actualSettings = s);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			nextSession.VerifySet(s => s.Settings = defaultSettings);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, actualSettings);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
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
		public void MustNotAbortIfNotWishedByUser()
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
		public void MustNotAllowToAbortIfNotInConfigureClientMode()
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
		public void MustNotFailWithoutCommandLineArgs()
		{
			var actualSettings = default(Settings);
			var defaultSettings = new Settings();
			var result = OperationResult.Failed;

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);
			nextSession.SetupSet<Settings>(s => s.Settings = It.IsAny<Settings>()).Callback(s => actualSettings = s);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, actualSettings);

			sut = new ConfigurationOperation(new string[] { }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			result = sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(defaultSettings, actualSettings);
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";
			var sut = new ConfigurationOperation(new[] { "blubb.exe", uri }, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var count = 0;
			var localSettings = new Settings { AdminPasswordHash = "1234" };
			var settings = new Settings { AdminPasswordHash = "9876", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.LocalPath.Contains("SettingsDummy")), out localSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			nextSession.SetupGet(s => s.Settings).Returns(settings);

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
		public void MustOnlyAllowToEnterSettingsPasswordFiveTimes()
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
		public void MustSucceedIfAdminPasswordCorrect()
		{
			var password = "test";
			var currentSettings = new Settings { AdminPasswordHash = "1234", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var nextSettings = new Settings { AdminPasswordHash = "9876", ConfigurationMode = ConfigurationMode.ConfigureClient };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			appConfig.AppDataFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));
			nextSession.SetupGet(s => s.Settings).Returns(nextSettings);

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
		public void MustSucceedIfSettingsPasswordCorrect()
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
		public void MustAbortAskingForAdminPasswordIfDecidedByUser()
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
		public void MustAbortAskingForSettingsPasswordIfDecidedByUser()
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
		public void MustReconfigureForExamWithCorrectUri()
		{
			var currentSettings = new Settings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "SettingsDummy.txt"));
			var settings = new Settings { ConfigurationMode = ConfigurationMode.Exam };

			currentSession.SetupGet(s => s.Settings).Returns(currentSettings);
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			nextSession.VerifySet(s => s.Settings = settings, Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustReconfigureForClientConfigurationWithCorrectUri()
		{
			var currentSettings = new Settings();
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "SettingsDummy.txt"));
			var settings = new Settings { ConfigurationMode = ConfigurationMode.ConfigureClient };

			currentSession.SetupGet(s => s.Settings).Returns(currentSettings);
			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			repository.Setup(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>())).Returns(SaveStatus.Success);

			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Repeat();

			nextSession.VerifySet(s => s.Settings = settings, Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, It.IsAny<PasswordParameters>()), Times.AtLeastOnce);
			repository.Verify(r => r.ConfigureClientWith(It.Is<Uri>(u => u.Equals(resource)), It.IsAny<PasswordParameters>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustFailToReconfigureWithInvalidUri()
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
		public void MustDoNothingOnRevert()
		{
			var sut = new ConfigurationOperation(null, repository.Object, hashAlgorithm.Object, logger.Object, sessionContext);
			var result = sut.Revert();

			currentSession.VerifyNoOtherCalls();
			hashAlgorithm.VerifyNoOtherCalls();
			nextSession.VerifyNoOtherCalls();
			repository.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
