/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.SystemComponents.Contracts.Registry;
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
		private Mock<IRegistry> registry;

		private ApplicationFactory sut;

		[TestInitialize]
		public void Initialize()
		{
			applicationMonitor = new Mock<IApplicationMonitor>();
			logger = new Mock<IModuleLogger>();
			nativeMethods = new Mock<INativeMethods>();
			processFactory = new Mock<IProcessFactory>();
			registry = new Mock<IRegistry>();

			sut = new ApplicationFactory(applicationMonitor.Object, logger.Object, nativeMethods.Object, processFactory.Object, registry.Object);
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
		public void MustCorrectlyReadPathFromRegistry()
		{
			object o = @"C:\Some\Registry\Path";
			var settings = new WhitelistApplication
			{
				DisplayName = "Windows Command Prompt",
				ExecutableName = "cmd.exe",
				ExecutablePath = @"C:\Some\Path"
			};

			registry.Setup(r => r.TryRead(It.Is<string>(s => s.Contains(RegistryValue.MachineHive.AppPaths_Key)), It.Is<string>(s => s == "Path"), out o)).Returns(true);

			var result = sut.TryCreate(settings, out var application);

			registry.Verify(r => r.TryRead(It.Is<string>(s => s.Contains(RegistryValue.MachineHive.AppPaths_Key)), It.Is<string>(s => s == "Path"), out o), Times.Once);

			Assert.AreEqual(FactoryResult.Success, result);
			Assert.IsNotNull(application);
			Assert.IsInstanceOfType<ExternalApplication>(application);
		}

		[TestMethod]
		public void MustIndicateIfApplicationNotFound()
		{
			var settings = new WhitelistApplication
			{
				ExecutableName = "some_random_application_which_does_not_exist_on_a_normal_system.exe",
				ExecutablePath = "Some/Path/Which/Does/Not/Exist"
			};

			var result = sut.TryCreate(settings, out var application);

			Assert.AreEqual(FactoryResult.NotFound, result);
			Assert.IsNull(application);
		}

		[TestMethod]
		public void MustFailGracefullyWhenPathIsInvalid()
		{
			var settings = new WhitelistApplication
			{
				ExecutableName = "asdfg(/ç)&=%\"fsdg..exe..",
				ExecutablePath = "[]#°§¬#°¢@tu03450'w89tz!$£äöüèé:"
			};

			var result = sut.TryCreate(settings, out _);

			logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);

			Assert.AreEqual(FactoryResult.NotFound, result);
		}

		[TestMethod]
		public void MustFailGracefullyAndIndicateThatErrorOccurred()
		{
			var o = default(object);
			var settings = new WhitelistApplication();

			registry.Setup(r => r.TryRead(It.IsAny<string>(), It.IsAny<string>(), out o)).Throws<Exception>();

			var result = sut.TryCreate(settings, out var application);

			Assert.AreEqual(FactoryResult.Error, result);
		}
	}
}
