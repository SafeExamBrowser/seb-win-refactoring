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
		private Mock<IRuntimeInfo> info;
		private Mock<ISettingsRepository> repository;
		private Mock<ISettings> settings;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			info = new Mock<IRuntimeInfo>();
			repository = new Mock<ISettingsRepository>();
			settings = new Mock<ISettings>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			info.SetupGet(i => i.AppDataFolder).Returns(@"C:\Not\Really\AppData");
			info.SetupGet(i => i.DefaultSettingsFileName).Returns("SettingsDummy.txt");
			info.SetupGet(i => i.ProgramDataFolder).Returns(@"C:\Not\Really\ProgramData");
			repository.Setup(r => r.Load(It.IsAny<Uri>())).Returns(settings.Object);
			repository.Setup(r => r.LoadDefaults()).Returns(settings.Object);
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			repository.Setup(r => r.LoadDefaults());

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, new string[] { })
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			repository.Verify(r => r.LoadDefaults(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var path = @"an/invalid\path.'*%yolo/()";

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, new [] { "blubb.exe", path })
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();
		}

		[TestMethod]
		public void MustUseCommandLineArgumentAs1stPrio()
		{
			var path = @"http://www.safeexambrowser.org/whatever.seb";
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.ProgramDataFolder).Returns(location);
			info.SetupGet(r => r.AppDataFolder).Returns(location);

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, new[] { "blubb.exe", path })
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(path)))), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.ProgramDataFolder).Returns(location);
			info.SetupGet(r => r.AppDataFolder).Returns($@"{location}\WRONG");

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.AppDataFolder).Returns(location);

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			repository.Verify(r => r.LoadDefaults(), Times.Once);
		}

		[TestMethod]
		public void MustAbortIfWishedByUser()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.ProgramDataFolder).Returns(location);
			uiFactory.Setup(u => u.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.Yes);

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			Assert.IsTrue(sut.AbortStartup);
		}

		[TestMethod]
		public void MustNotAbortIfNotWishedByUser()
		{
			uiFactory.Setup(u => u.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>())).Returns(MessageBoxResult.No);

			sut = new ConfigurationOperation(logger.Object, info.Object, repository.Object, text.Object, uiFactory.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			Assert.IsFalse(sut.AbortStartup);
		}
	}
}
