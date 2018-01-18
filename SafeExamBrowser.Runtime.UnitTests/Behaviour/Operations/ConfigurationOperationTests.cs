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
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Runtime;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IRuntimeController> controller;
		private Mock<IRuntimeInfo> info;
		private Mock<ISettingsRepository> repository;
		private Mock<ISplashScreen> splashScreen;

		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			controller = new Mock<IRuntimeController>();
			info = new Mock<IRuntimeInfo>();
			repository = new Mock<ISettingsRepository>();
			splashScreen = new Mock<ISplashScreen>();

			info.SetupGet(r => r.AppDataFolder).Returns(@"C:\Not\Really\AppData");
			info.SetupGet(r => r.DefaultSettingsFileName).Returns("SettingsDummy.txt");
			info.SetupGet(r => r.ProgramDataFolder).Returns(@"C:\Not\Really\ProgramData");
		}

		[TestMethod]
		public void MustNotFailWithoutCommandLineArgs()
		{
			controller.SetupSet(c => c.Settings = It.IsAny<ISettings>());

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, new string[] { })
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			controller.VerifySet(c => c.Settings = It.IsAny<ISettings>(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWithInvalidUri()
		{
			var path = @"an/invalid\path.'*%yolo/()";

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, new [] { "blubb.exe", path })
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

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, new[] { "blubb.exe", path })
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			controller.VerifySet(c => c.Settings = It.IsAny<ISettings>(), Times.Once);
			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(path)))), Times.Once);
		}

		[TestMethod]
		public void MustUseProgramDataAs2ndPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.ProgramDataFolder).Returns(location);
			info.SetupGet(r => r.AppDataFolder).Returns($@"{location}\WRONG");

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			controller.VerifySet(c => c.Settings = It.IsAny<ISettings>(), Times.Once);
			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustUseAppDataAs3rdPrio()
		{
			var location = Path.GetDirectoryName(GetType().Assembly.Location);

			info.SetupGet(r => r.AppDataFolder).Returns(location);

			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			controller.VerifySet(c => c.Settings = It.IsAny<ISettings>(), Times.Once);
			repository.Verify(r => r.Load(It.Is<Uri>(u => u.Equals(new Uri(Path.Combine(location, "SettingsDummy.txt"))))), Times.Once);
		}

		[TestMethod]
		public void MustFallbackToDefaultsAsLastPrio()
		{
			sut = new ConfigurationOperation(logger.Object, controller.Object, info.Object, repository.Object, null)
			{
				SplashScreen = splashScreen.Object
			};

			sut.Perform();

			controller.VerifySet(c => c.Settings = It.IsAny<ISettings>(), Times.Once);
			repository.Verify(r => r.LoadDefaults(), Times.Once);
		}
	}
}
