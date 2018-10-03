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
using SafeExamBrowser.Contracts.Communication.Proxies;
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
		private Mock<IResourceLoader> resourceLoader;
		private Settings settings;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			repository = new Mock<IConfigurationRepository>();
			resourceLoader = new Mock<IResourceLoader>();
			settings = new Settings();

			appConfig.AppDataFolder = @"C:\Not\Really\AppData";
			appConfig.DefaultSettingsFileName = "SettingsDummy.txt";
			appConfig.ProgramDataFolder = @"C:\Not\Really\ProgramData";

			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = location;

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.Perform();

			var resource = new Uri(url);

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.ProgramDataFolder = location;
			appConfig.AppDataFolder = $@"{location}\WRONG";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));

			appConfig.AppDataFolder = location;

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			appConfig.ProgramDataFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), nameof(Operations));
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
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
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
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
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
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
			repository.Setup(r => r.LoadDefaultSettings());

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);
			sut.Perform();

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new string[] { });
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", uri });
			sut.Perform();
		}

		[TestMethod]
		public void MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustSucceedIfAdminPasswordCorrect()
		{
			var password = "test";
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), password, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = password;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), password, null), Times.Once);
		}

		[TestMethod]
		public void MustSucceedIfSettingsPasswordCorrect()
		{
			var password = "test";
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, password)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = password;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, password), Times.Once);
		}

		[TestMethod]
		public void MustAbortAskingForAdminPasswordIfDecidedByUser()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
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

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
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

			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, settingsPassword)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), adminPassword, settingsPassword)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
			sut.ActionRequired += args =>
			{
				if (args is PasswordRequiredEventArgs p)
				{
					p.Password = p.Purpose == PasswordRequestPurpose.Administrator ? adminPassword : settingsPassword;
					p.Success = true;
				}
			};

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, settingsPassword), Times.Once);
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), adminPassword, settingsPassword), Times.Once);
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

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustHandleInvalidData()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			resourceLoader.Setup(r => r.IsHtmlResource(It.IsAny<Uri>())).Returns(false);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.InvalidData);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustHandleHtmlAsInvalidData()
		{
			var url = "http://www.blubb.org/some/resource.html";

			resourceLoader.Setup(r => r.IsHtmlResource(It.IsAny<Uri>())).Returns(true);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.InvalidData);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(url, settings.Browser.StartUrl);
		}

		[TestMethod]
		public void MustReconfigureSuccessfullyWithCorrectUri()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);
			var resource = new Uri(Path.Combine(location, nameof(Operations), "SettingsDummy.txt"));

			repository.SetupGet(r => r.ReconfigurationFilePath).Returns(resource.AbsolutePath);
			repository.Setup(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);

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

			sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, null);

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
