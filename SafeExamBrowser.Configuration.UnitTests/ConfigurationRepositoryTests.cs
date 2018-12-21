/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.UnitTests
{
	[TestClass]
	public class ConfigurationRepositoryTests
	{
		private ConfigurationRepository sut;

		[TestInitialize]
		public void Initialize()
		{
			var executablePath = Assembly.GetExecutingAssembly().Location;
			var hashAlgorithm = new Mock<IHashAlgorithm>();
			var logger = new Mock<IModuleLogger>();

			sut = new ConfigurationRepository(hashAlgorithm.Object, logger.Object, executablePath, string.Empty, string.Empty, string.Empty);
		}

		[TestMethod]
		public void MustCorrectlyInitializeSessionConfiguration()
		{
			var appConfig = sut.InitializeAppConfig();
			var configuration = sut.InitializeSessionConfiguration();

			Assert.IsInstanceOfType(configuration.AppConfig, typeof(AppConfig));
			Assert.IsInstanceOfType(configuration.Id, typeof(Guid));
			Assert.IsNull(configuration.Settings);
			Assert.IsInstanceOfType(configuration.StartupToken, typeof(Guid));
		}

		[TestMethod]
		public void MustCorrectlyUpdateAppConfig()
		{
			var appConfig = sut.InitializeAppConfig();
			var clientAddress = appConfig.ClientAddress;
			var clientId = appConfig.ClientId;
			var clientLogFile = appConfig.ClientLogFile;
			var runtimeAddress = appConfig.RuntimeAddress;
			var runtimeId = appConfig.RuntimeId;
			var runtimeLogFile = appConfig.RuntimeLogFile;
			var configuration = sut.InitializeSessionConfiguration();

			Assert.AreNotEqual(configuration.AppConfig.ClientAddress, clientAddress);
			Assert.AreNotEqual(configuration.AppConfig.ClientId, clientId);
			Assert.AreEqual(configuration.AppConfig.ClientLogFile, clientLogFile);
			Assert.AreEqual(configuration.AppConfig.RuntimeAddress, runtimeAddress);
			Assert.AreEqual(configuration.AppConfig.RuntimeId, runtimeId);
			Assert.AreEqual(configuration.AppConfig.RuntimeLogFile, runtimeLogFile);
		}

		[TestMethod]
		public void MustCorrectlyUpdateSessionConfiguration()
		{
			var appConfig = sut.InitializeAppConfig();
			var firstSession = sut.InitializeSessionConfiguration();
			var secondSession = sut.InitializeSessionConfiguration();
			var thirdSession = sut.InitializeSessionConfiguration();

			Assert.AreNotEqual(firstSession.Id, secondSession.Id);
			Assert.AreNotEqual(firstSession.StartupToken, secondSession.StartupToken);
			Assert.AreNotEqual(secondSession.Id, thirdSession.Id);
			Assert.AreNotEqual(secondSession.StartupToken, thirdSession.StartupToken);
		}
	}
}
