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
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.WindowsApi;

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

			sut = new ConfigurationRepository(executablePath, string.Empty, string.Empty, string.Empty);
		}

		[TestMethod]
		public void AppConfigMustNeverBeNull()
		{
			Assert.IsNotNull(sut.AppConfig);
		}

		[TestMethod]
		public void CurrentSessionIsInitiallyNull()
		{
			Assert.IsNull(sut.CurrentSession);
		}

		[TestMethod]
		public void CurrentSettingsAreInitiallyNull()
		{
			Assert.IsNull(sut.CurrentSettings);
		}

		[TestMethod]
		public void MustCorrectlyBuildClientConfiguration()
		{
			sut.LoadDefaultSettings();
			sut.InitializeSessionConfiguration();

			var appConfig = sut.AppConfig;
			var clientConfig = sut.BuildClientConfiguration();
			var session = sut.CurrentSession;
			var settings = sut.CurrentSettings;

			Assert.AreEqual(session.Id, clientConfig.SessionId);
			Assert.AreSame(appConfig, clientConfig.AppConfig);
			Assert.AreSame(settings, clientConfig.Settings);
		}

		[TestMethod]
		public void MustCorrectlyInitializeSessionConfiguration()
		{
			sut.InitializeSessionConfiguration();

			Assert.IsNull(sut.CurrentSession.ClientProcess);
			Assert.IsNull(sut.CurrentSession.ClientProxy);
			Assert.IsInstanceOfType(sut.CurrentSession.Id, typeof(Guid));
			Assert.IsInstanceOfType(sut.CurrentSession.StartupToken, typeof(Guid));
		}

		[TestMethod]
		public void MustCorrectlyUpdateAppConfig()
		{
			var clientAddress = sut.AppConfig.ClientAddress;
			var clientId = sut.AppConfig.ClientId;
			var clientLogFile = sut.AppConfig.ClientLogFile;
			var runtimeAddress = sut.AppConfig.RuntimeAddress;
			var runtimeId = sut.AppConfig.RuntimeId;
			var runtimeLogFile = sut.AppConfig.RuntimeLogFile;

			sut.InitializeSessionConfiguration();

			Assert.AreNotEqual(sut.AppConfig.ClientAddress, clientAddress);
			Assert.AreNotEqual(sut.AppConfig.ClientId, clientId);
			Assert.AreEqual(sut.AppConfig.ClientLogFile, clientLogFile);
			Assert.AreEqual(sut.AppConfig.RuntimeAddress, runtimeAddress);
			Assert.AreEqual(sut.AppConfig.RuntimeId, runtimeId);
			Assert.AreEqual(sut.AppConfig.RuntimeLogFile, runtimeLogFile);
		}

		[TestMethod]
		public void MustCorrectlyUpdateSessionConfiguration()
		{
			var process = new Mock<IProcess>();
			var proxy = new Mock<IClientProxy>();

			sut.InitializeSessionConfiguration();

			var firstSession = sut.CurrentSession;

			sut.CurrentSession.ClientProcess = process.Object;
			sut.CurrentSession.ClientProxy = proxy.Object;
			sut.InitializeSessionConfiguration();

			var secondSession = sut.CurrentSession;

			Assert.AreSame(firstSession.ClientProcess, secondSession.ClientProcess);
			Assert.AreSame(firstSession.ClientProxy, secondSession.ClientProxy);
			Assert.AreNotEqual(firstSession.Id, secondSession.Id);
			Assert.AreNotEqual(firstSession.StartupToken, secondSession.StartupToken);

			sut.CurrentSession.ClientProcess = null;
			sut.CurrentSession.ClientProxy = null;
			sut.InitializeSessionConfiguration();

			var thirdSession = sut.CurrentSession;

			Assert.IsNull(thirdSession.ClientProcess);
			Assert.IsNull(thirdSession.ClientProxy);
			Assert.AreNotEqual(secondSession.Id, thirdSession.Id);
			Assert.AreNotEqual(secondSession.StartupToken, thirdSession.StartupToken);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullForExecutablePath()
		{
			new ConfigurationRepository(null, null, null, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullForProgramCopyright()
		{
			new ConfigurationRepository(string.Empty, null, null, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullForProgramTitle()
		{
			new ConfigurationRepository(string.Empty, string.Empty, null, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullForProgramVersion()
		{
			new ConfigurationRepository(string.Empty, string.Empty, string.Empty, null);
		}
	}
}
