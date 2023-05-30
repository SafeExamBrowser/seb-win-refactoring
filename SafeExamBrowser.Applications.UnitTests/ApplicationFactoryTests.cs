/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications.UnitTests
{
	[TestClass]
	public class ApplicationFactoryTests
	{
		private Mock<IApplicationMonitor> applicationMonitor;
		private Mock<IModuleLogger> logger;
		private Mock<INativeMethods> nativeMethods;
		private Mock<IProcessFactory> processFactory;

		private ApplicationFactory sut;

		[TestInitialize]
		public void Initialize()
		{
			applicationMonitor = new Mock<IApplicationMonitor>();
			logger = new Mock<IModuleLogger>();
			nativeMethods = new Mock<INativeMethods>();
			processFactory = new Mock<IProcessFactory>();

			sut = new ApplicationFactory(applicationMonitor.Object, logger.Object, nativeMethods.Object, processFactory.Object);
		}

		[TestMethod]
		public void MustCorrectlyCreateApplication()
		{
			var settings = new WhitelistApplication
			{
				DisplayName = "Windows Command Prompt",
				ExecutableName = "cmd.exe",
			};

			var result = sut.TryCreate(settings, out var application);

			Assert.AreEqual(FactoryResult.Success, result);
			Assert.IsNotNull(application);
			Assert.IsInstanceOfType<ExternalApplication>(application);
		}

		[TestMethod]
		public void MustIndicateIfApplicationNotFound()
		{
			var settings = new WhitelistApplication
			{
				ExecutableName = "some_random_application_which_does_not_exist_on_a_normal_system.exe"
			};

			var result = sut.TryCreate(settings, out var application);

			Assert.AreEqual(FactoryResult.NotFound, result);
			Assert.IsNull(application);
		}

		[TestMethod]
		public void MustFailGracefullyAndIndicateThatErrorOccurred()
		{
			var settings = new WhitelistApplication
			{
				DisplayName = "Windows Command Prompt",
				ExecutableName = "cmd.exe",
			};

			logger.Setup(l => l.CloneFor(It.IsAny<string>())).Throws<Exception>();

			var result = sut.TryCreate(settings, out var application);

			Assert.AreEqual(FactoryResult.Error, result);
		}
	}
}
