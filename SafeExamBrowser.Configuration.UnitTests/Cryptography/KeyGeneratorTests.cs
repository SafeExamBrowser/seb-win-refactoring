/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.Cryptography
{
	[TestClass]
	public class KeyGeneratorTests
	{
		private AppConfig appConfig;
		private Mock<IIntegrityModule> integrityModule;
		private Mock<ILogger> logger;
		private KeyGenerator sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			integrityModule = new Mock<IIntegrityModule>();
			logger = new Mock<ILogger>();

			sut = new KeyGenerator(appConfig, integrityModule.Object, logger.Object);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
		public void CalculateBrowserExamKeyHash_MustFailWithoutUrl()
		{
			sut.CalculateBrowserExamKeyHash(default, default, default);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
		public void CalculateConfigurationKeyHash_MustFailWithoutUrl()
		{
			sut.CalculateConfigurationKeyHash(default, default);
		}

		[TestMethod]
		public void MustAllowForConcurrentKeyHashCalculation()
		{
			Parallel.For(0, 1000, (_) =>
			{
				sut.CalculateBrowserExamKeyHash(default, default, "https://www.safeexambrowser.org");
				sut.CalculateConfigurationKeyHash(default, "https://www.safeexambrowser.org");
			});
		}
	}
}
