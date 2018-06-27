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
using SafeExamBrowser.Contracts.Communication.Hosts;
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
		private RuntimeInfo info;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IPasswordDialog> passwordDialog;
		private Mock<IConfigurationRepository> repository;
		private Mock<IRuntimeHost> runtimeHost;
		private Settings settings;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			info = new RuntimeInfo();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			passwordDialog = new Mock<IPasswordDialog>();
			repository = new Mock<IConfigurationRepository>();
			runtimeHost = new Mock<IRuntimeHost>();
			settings = new Settings();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			info.AppDataFolder = @"C:\Not\Really\AppData";
			info.DefaultSettingsFileName = "SettingsDummy.txt";
			info.ProgramDataFolder = @"C:\Not\Really\ProgramData";

			uiFactory.Setup(f => f.CreatePasswordDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordDialog.Object);
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var url = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.ProgramDataFolder = location;
			info.AppDataFolder = location;

			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			var resource = new Uri(url);

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.ProgramDataFolder = location;
			info.AppDataFolder = $@"{location}\WRONG";

			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.AppDataFolder = location;

			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);
			sut.Perform();

			var resource = new Uri(Path.Combine(location, "SettingsDummy.txt"));

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(resource)), null, null), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			info.ProgramDataFolder = Path.GetDirectoryName(GetType().Assembly.Location);
			messageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.Yes);
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustNotAbortIfNotWishedByUser()
		{
			messageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.No);
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustNotAllowToAbortIfNotInConfigureClientMode()
		{
			settings.ConfigurationMode = ConfigurationMode.Exam;
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);
			sut.Perform();

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>()), Times.Never);
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			repository.Setup(r => r.LoadDefaultSettings());

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, null);
			sut.Perform();

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new string[] { });
			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var uri = @"an/invalid\uri.'*%yolo/()你好";

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", uri });
			sut.Perform();
		}

		[TestMethod]
		public void MustOnlyAllowToEnterAdminPasswordFiveTimes()
		{
			var result = new PasswordDialogResultStub { Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(5));
		}

		[TestMethod]
		public void MustOnlyAllowToEnterSettingsPasswordFiveTimes()
		{
			var result = new PasswordDialogResultStub { Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
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
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.AdminPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), password, null)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(1));
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), password, null), Times.Exactly(1));
		}

		[TestMethod]
		public void MustSucceedIfSettingsPasswordCorrect()
		{
			var password = "test";
			var result = new PasswordDialogResultStub { Password = password, Success = true };
			var url = @"http://www.safeexambrowser.org/whatever.seb";

			passwordDialog.Setup(d => d.Show(null)).Returns(result);
			repository.SetupGet(r => r.CurrentSettings).Returns(settings);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, password)).Returns(LoadStatus.Success);

			sut = new ConfigurationOperation(repository.Object, logger.Object, messageBox.Object, runtimeHost.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", url });
			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, null), Times.Exactly(1));
			repository.Verify(r => r.LoadSettings(It.IsAny<Uri>(), null, password), Times.Exactly(1));
		}
	}
}
