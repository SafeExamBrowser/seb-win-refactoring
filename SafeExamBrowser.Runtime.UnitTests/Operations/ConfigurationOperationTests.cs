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
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Configuration;
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
		private Mock<ILogger> logger;
		private Mock<IConfigurationRepository> repository;
		private Mock<ISessionConfiguration> session;
		private SessionContext sessionContext;
		private Settings settings;

		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			repository = new Mock<IConfigurationRepository>();
			session = new Mock<ISessionConfiguration>();
			sessionContext = new SessionContext();
			settings = new Settings();

			appConfig.AppDataFolder = @"C:\Not\Really\AppData";
			appConfig.DefaultSettingsFileName = "SettingsDummy.txt";
			appConfig.ProgramDataFolder = @"C:\Not\Really\ProgramData";
			session.SetupGet(s => s.AppConfig).Returns(appConfig);
			session.SetupGet(s => s.Settings).Returns(settings);
			sessionContext.Next = session.Object;
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var settings = default(Settings);
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			var resource = new Uri(url);

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = $@"{location}\WRONG";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.AppDataFolder = location;

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			var actualSettings = default(Settings);
			var defaultSettings = new Settings();

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);
			session.SetupSet<Settings>(s => s.Settings = It.IsAny<Settings>()).Callback(s => actualSettings = s);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
			session.VerifySet(s => s.Settings = defaultSettings);

			Assert.AreSame(defaultSettings, actualSettings);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			appConfig.ProgramDataFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
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
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
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
			settings.ConfigurationMode = ConfigurationMode.Exam;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is ConfigurationCompletedEventArgs c)
				{
					Assert.Fail();
				}
			};

			sut.Perform();
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			var actualSettings = default(Settings);
			var defaultSettings = new Settings();

			repository.Setup(r => r.LoadDefaultSettings()).Returns(defaultSettings);
			session.SetupSet<Settings>(s => s.Settings = It.IsAny<Settings>()).Callback(s => actualSettings = s);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);

			Assert.AreSame(defaultSettings, actualSettings);

			sut = new ConfigurationOperation(new string[] { }, repository.Object, logger.Object, sessionContext);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));

			Assert.AreSame(defaultSettings, actualSettings);
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";

			sut = new ConfigurationOperation(new[] { "blubb.exe", uri }, repository.Object, logger.Object, sessionContext);
			sut.Perform();
		}

		[TestMethod]
		public void MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustSucceedIfAdminPasswordCorrect()
		{
			var password = "test";
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, password, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = password;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, password, null), Times.Once);
		}

		[TestMethod]
		public void MustSucceedIfSettingsPasswordCorrect()
		{
			var password = "test";
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, password)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = password;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, password), Times.Once);
		}

		[TestMethod]
		public void MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
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
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
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
		public void MustAllowEnteringBothPasswords()
		{
			var adminPassword = "xyz";
			var settingsPassword = "abc";
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, settingsPassword)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, adminPassword, settingsPassword)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, sessionContext);
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = p.Purpose == PasswordRequestPurpose.Administrator ? adminPassword : settingsPassword;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, settingsPassword), Times.Once);
			repository.Verify(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, adminPassword, settingsPassword), Times.Once);
		}

		[TestMethod]
		public void MustReconfigureSuccessfullyWithCorrectUri()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "SettingsDummy.txt"));

			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			repository.Setup(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);

			var result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustFailToReconfigureWithInvalidUri()
		{
			var resource = new Uri("file:///C:/does/not/exist.txt");

			sessionContext.ReconfigurationFilePath = null;
			repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), out settings, null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(null, repository.Object, logger.Object, sessionContext);

			var result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);

			sessionContext.ReconfigurationFilePath = resource.LocalPath;
			result = sut.Repeat();

			repository.Verify(r => r.TryLoadSettings(It.Is<Uri>(u => u.Equals(resource)), out settings, null, null), Times.Never);
			Assert.AreEqual(OperationResult.Failed, result);
		}
	}
}
