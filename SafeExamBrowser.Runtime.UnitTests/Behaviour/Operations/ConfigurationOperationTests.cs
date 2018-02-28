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
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private Mock<ILogger> logger;
		private RuntimeInfo info;
		private Mock<IConfigurationRepository> repository;
		private Settings settings;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			info = new RuntimeInfo();
			repository = new Mock<IConfigurationRepository>();
			settings = new Settings();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			info.AppDataFolder = @"C:\Not\Really\AppData";
			info.DefaultSettingsFileName = "SettingsDummy.txt";
			info.ProgramDataFolder = @"C:\Not\Really\ProgramData";
			repository.Setup(r => r.LoadSettings(It.IsAny<Uri>())).Returns(settings);
			repository.Setup(r => r.LoadDefaultSettings()).Returns(settings);
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			repository.Setup(r => r.LoadDefaultSettings());

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			sut.Perform();

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, new string[] { });

			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var path = @"an/invalid\path.'*%yolo/()";

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, new [] { "blubb.exe", path });

			sut.Perform();
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var path = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.ProgramDataFolder = location;
			info.AppDataFolder = location;

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, new[] { "blubb.exe", path });

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(new Uri(path)))), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.ProgramDataFolder = location;
			info.AppDataFolder = $@"{location}\WRONG";

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.AppDataFolder = location;

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			sut.Perform();

			repository.Verify(r => r.LoadSettings(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			sut.Perform();

			repository.Verify(r => r.LoadDefaultSettings(), Times.Once);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			info.ProgramDataFolder = Path.GetDirectoryName(GetType().Assembly.Location);
			uiFactory.Setup(u => u.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.Yes);

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustNotAbortIfNotWishedByUser()
		{
			uiFactory.Setup(u => u.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.No);

			sut = new ConfigurationOperation(repository.Object, logger.Object, info, text.Object, uiFactory.Object, null);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
