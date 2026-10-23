/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Bootstrap;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class CompositionRootTests
	{
		private const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

		private AppConfig appConfig;
		private List<string> entries;
		private Mock<ILogger> logger;
		private Mock<ISystemInfo> systemInfo;

		private CompositionRoot sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig
			{
				ApplicationStartTime = new DateTime(2026, 10, 22, 8, 15, 30, 250),
				ProgramBuildVersion = "3.10.0.456",
				ProgramCopyright = "Copyright (c) 2026 ETH Zürich, IT Services",
				ProgramInformationalVersion = "3.10.0",
				ProgramTitle = "Safe Exam Browser",
				RuntimeId = Guid.NewGuid()
			};
			entries = new List<string>();
			logger = new Mock<ILogger>();
			systemInfo = new Mock<ISystemInfo>();

			logger.Setup(l => l.Log(It.IsAny<string>())).Callback<string>(m => entries.Add(m));
			systemInfo.SetupGet(s => s.Manufacturer).Returns("Example Inc.");
			systemInfo.SetupGet(s => s.Model).Returns("Model X");
			systemInfo.SetupGet(s => s.Name).Returns("EXAM-PC-01");
			systemInfo.SetupGet(s => s.OperatingSystemInfo).Returns("Windows 11, Version 24H2, x64");

			sut = new CompositionRoot();

			Inject("appConfig", appConfig);
			Inject("logger", logger.Object);
			Inject("systemInfo", systemInfo.Object);
		}

		[TestMethod]
		public void BuildObjectGraph_MustInstantiateAndWireAllComponents()
		{
			var factory = new Mock<IUserInterfaceFactory>();
			var runtimeWindow = new Mock<IRuntimeWindow>();
			var splashScreen = new Mock<ISplashScreen>();
			var root = new TestableCompositionRoot(factory.Object, systemInfo.Object);

			factory.Setup(f => f.CreateRuntimeWindow(It.IsAny<AppConfig>())).Returns(runtimeWindow.Object);
			factory.Setup(f => f.CreateSplashScreen(It.IsAny<AppConfig>())).Returns(splashScreen.Object);

			root.BuildObjectGraph(() => { });

			Assert.IsNotNull(root.RuntimeController);

			factory.Verify(f => f.CreateRuntimeWindow(It.IsAny<AppConfig>()), Times.Once);
			factory.Verify(f => f.CreateSplashScreen(It.IsAny<AppConfig>()), Times.Once);

			AssertBootstrapOperations(root.RuntimeController);
			AssertResponsibilities(root.RuntimeController);
			AssertSessionOperations(root.RuntimeController);
		}

		[TestMethod]
		public void LogStartupInformation_MustLogProgramInformation()
		{
			sut.LogStartupInformation();

			Assert.Contains("/* Safe Exam Browser, Version 3.10.0, Build 3.10.0.456", entries);
			Assert.Contains("/* Copyright (c) 2026 ETH Zürich, IT Services", entries);
		}

		[TestMethod]
		public void LogStartupInformation_MustLogApplicationStartTime()
		{
			sut.LogStartupInformation();

			Assert.Contains($"# Application started at {appConfig.ApplicationStartTime.ToString(TIMESTAMP_FORMAT)}", entries);
		}

		[TestMethod]
		public void LogStartupInformation_MustLogSystemInformation()
		{
			sut.LogStartupInformation();

			Assert.Contains("# Running on Windows 11, Version 24H2, x64", entries);
			Assert.Contains("# Computer 'EXAM-PC-01' is a Model X manufactured by Example Inc.", entries);
		}

		[TestMethod]
		public void LogStartupInformation_MustLogRuntimeId()
		{
			sut.LogStartupInformation();

			Assert.Contains($"# Runtime-ID: {appConfig.RuntimeId}", entries);
		}

		[TestMethod]
		public void LogStartupInformation_MustStartWithProgramInformationAndEndWithEmptyLine()
		{
			sut.LogStartupInformation();

			Assert.StartsWith("/* Safe Exam Browser", entries.First());
			Assert.AreEqual(string.Empty, entries.Last());
		}

		[TestMethod]
		public void LogStartupInformation_MustOnlyWriteRawEntries()
		{
			sut.LogStartupInformation();

			logger.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeastOnce);
			logger.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void LogShutdownInformation_MustLogTerminationTime()
		{
			var prefix = "# Application terminated at ";

			sut.LogShutdownInformation();

			var entry = entries.Single();

			Assert.StartsWith(prefix, entry);
			Assert.IsTrue(DateTime.TryParseExact(entry.Substring(prefix.Length), TIMESTAMP_FORMAT, CultureInfo.CurrentCulture, DateTimeStyles.None, out var timestamp));
			Assert.IsLessThan(5, Math.Abs((DateTime.Now - timestamp).TotalSeconds));
		}

		[TestMethod]
		public void LogShutdownInformation_MustOnlyWriteRawEntries()
		{
			sut.LogShutdownInformation();

			logger.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
			logger.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void LogShutdownInformation_MustNotFailWithoutObjectGraph()
		{
			var uninitialized = new CompositionRoot();

			uninitialized.LogShutdownInformation();
		}

		private static void AssertBootstrapOperations(RuntimeController controller)
		{
			var sequence = GetField<object>(controller, "bootstrapSequence");
			var operations = GetField<IEnumerable>(sequence, "operations");

			Assert.AreSequenceEqual(new[]
			{
				typeof(I18nOperation),
				typeof(CrashLogOperation),
				typeof(CommunicationHostOperation),
				typeof(IntegrityOperation)
			}, TypesOf(operations));
		}

		private static void AssertResponsibilities(RuntimeController controller)
		{
			var collection = GetField<object>(controller, "responsibilities");
			var responsibilities = GetField<IEnumerable>(collection, "responsibilities");

			Assert.AreSequenceEqual(new[]
			{
				typeof(ClientResponsibility),
				typeof(CommunicationResponsibility),
				typeof(ErrorMessageResponsibility),
				typeof(IntegrityResponsibility),
				typeof(ServiceResponsibility),
				typeof(SessionResponsibility)
			}, TypesOf(responsibilities));
		}

		private static void AssertSessionOperations(RuntimeController controller)
		{
			var collection = GetField<object>(controller, "responsibilities");
			var responsibilities = GetField<IEnumerable>(collection, "responsibilities");
			var sessionResponsibility = responsibilities.OfType<SessionResponsibility>().Single();
			var sequence = GetField<object>(sessionResponsibility, "sessionSequence");
			var operations = GetField<IEnumerable>(sequence, "operations");

			Assert.AreSequenceEqual(new[]
			{
				typeof(SessionInitializationOperation),
				typeof(ConfigurationOperation),
				typeof(ServerOperation),
				typeof(VersionRestrictionOperation),
				typeof(DisclaimerOperation),
				typeof(RemoteSessionOperation),
				typeof(SessionIntegrityOperation),
				typeof(VirtualMachineOperation),
				typeof(DisplayMonitorOperation),
				typeof(ServiceOperation),
				typeof(ClientTerminationOperation),
				typeof(KioskModeOperation),
				typeof(ClientOperation),
				typeof(SessionActivationOperation)
			}, TypesOf(operations));
		}

		private static T GetField<T>(object instance, string fieldName)
		{
			var field = default(FieldInfo);

			for (var type = instance.GetType(); type != default && field == default; type = type.BaseType)
			{
				field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			}

			Assert.IsNotNull(field, $"Field '{fieldName}' not found - was it renamed in {instance.GetType().Name}?");

			return (T) field.GetValue(instance);
		}

		private void Inject(string fieldName, object value)
		{
			var field = typeof(CompositionRoot).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

			Assert.IsNotNull(field, $"Field '{fieldName}' not found - was it renamed in {nameof(CompositionRoot)}?");

			field.SetValue(sut, value);
		}

		private static Type[] TypesOf(IEnumerable items)
		{
			return items.Cast<object>().Select(i => i.GetType()).ToArray();
		}

		/// <summary>
		/// Replaces the environment-dependent parts of the object graph (WPF user interface, WMI queries, entry assembly, file system) with test
		/// doubles. Everything else is instantiated and wired exactly as in production.
		/// </summary>
		private class TestableCompositionRoot : CompositionRoot
		{
			private readonly IUserInterfaceFactory factory;
			private readonly ISystemInfo systemInfo;

			internal TestableCompositionRoot(IUserInterfaceFactory factory, ISystemInfo systemInfo)
			{
				this.factory = factory;
				this.systemInfo = systemInfo;
			}

			protected override ISystemInfo CreateSystemInfo(IRegistry registry)
			{
				return systemInfo;
			}

			protected override IUserInterfaceFactory CreateUserInterfaceFactory()
			{
				return factory;
			}

			protected override AppConfig InitializeAppConfig()
			{
				return new AppConfig
				{
					RuntimeAddress = $"{AppConfig.BASE_ADDRESS}/runtime/{Guid.NewGuid()}",
					ServiceAddress = AppConfig.SERVICE_ADDRESS,
					TemporaryDirectory = Path.GetTempPath()
				};
			}

			protected override void InitializeLogging()
			{
				// Prevents the creation of the log directory, the logger itself stays in place.
			}
		}
	}
}
