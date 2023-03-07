/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ApplicationOperationTests
	{
		private ClientContext context;
		private Mock<IApplicationFactory> factory;
		private Mock<IApplicationMonitor> monitor;
		private Mock<ILogger> logger;
		private Mock<IText> text;

		private ApplicationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			factory = new Mock<IApplicationFactory>();
			monitor = new Mock<IApplicationMonitor>();
			logger = new Mock<ILogger>();
			text = new Mock<IText>();

			context.Settings = new AppSettings();

			sut = new ApplicationOperation(context, factory.Object, monitor.Object, logger.Object, text.Object);
		}

		[TestMethod]
		public void Perform_MustAbortIfUserDeniesAutoTermination()
		{
			var initialization = new InitializationResult();
			var args = default(ActionRequiredEventArgs);

			initialization.RunningApplications.Add(new RunningApplication(default(string)));
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(initialization);
			sut.ActionRequired += (a) =>
			{
				args = a;

				if (a is ApplicationTerminationEventArgs t)
				{
					t.TerminateProcesses = false;
				}
			};

			var result = sut.Perform();

			monitor.Verify(m => m.Initialize(It.Is<ApplicationSettings>(s => s == context.Settings.Applications)), Times.Once);
			monitor.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Aborted, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationTerminationEventArgs));
		}

		[TestMethod]
		public void Perform_MustAbortIfUserCancelsApplicationLocationSelection()
		{
			var application = new Mock<IApplication>().Object;
			var applicationSettings = new WhitelistApplication { AllowCustomPath = true };
			var args = default(ActionRequiredEventArgs);

			context.Settings.Applications.Whitelist.Add(applicationSettings);
			factory.Setup(f => f.TryCreate(It.IsAny<WhitelistApplication>(), out application)).Returns(FactoryResult.NotFound);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());
			sut.ActionRequired += (a) =>
			{
				args = a;

				if (a is ApplicationNotFoundEventArgs n)
				{
					n.Success = false;
				}
			};

			var result = sut.Perform();

			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == applicationSettings), out application), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationInitializationFailedEventArgs));
		}

		[TestMethod]
		public void Perform_MustAllowUserToChooseApplicationLocation()
		{
			var application = new Mock<IApplication>().Object;
			var applicationSettings = new WhitelistApplication { AllowCustomPath = true };
			var args = default(ActionRequiredEventArgs);
			var attempt = 0;
			var correct = new Random().Next(2, 50);
			var factoryResult = new Func<FactoryResult>(() => ++attempt == correct ? FactoryResult.Success : FactoryResult.NotFound);

			context.Settings.Applications.Whitelist.Add(applicationSettings);
			factory.Setup(f => f.TryCreate(It.IsAny<WhitelistApplication>(), out application)).Returns(factoryResult);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());
			sut.ActionRequired += (a) =>
			{
				args = a;

				if (a is ApplicationNotFoundEventArgs n)
				{
					n.Success = true;
				}
			};

			var result = sut.Perform();

			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == applicationSettings), out application), Times.Exactly(correct));

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationNotFoundEventArgs));
		}

		[TestMethod]
		public void Perform_MustDenyApplicationLocationSelection()
		{
			var application = new Mock<IApplication>().Object;
			var applicationSettings = new WhitelistApplication { AllowCustomPath = false };
			var args = default(ActionRequiredEventArgs);

			context.Settings.Applications.Whitelist.Add(applicationSettings);
			factory.Setup(f => f.TryCreate(It.IsAny<WhitelistApplication>(), out application)).Returns(FactoryResult.NotFound);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());
			sut.ActionRequired += (a) =>
			{
				args = a;

				if (a is ApplicationNotFoundEventArgs)
				{
					Assert.Fail();
				}
			};

			var result = sut.Perform();

			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == applicationSettings), out application), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationInitializationFailedEventArgs));
		}

		[TestMethod]
		public void Perform_MustFailIfAutoTerminationFails()
		{
			var initialization = new InitializationResult();
			var args = default(ActionRequiredEventArgs);

			initialization.FailedAutoTerminations.Add(new RunningApplication(default(string)));
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(initialization);
			sut.ActionRequired += (a) => args = a;

			var result = sut.Perform();

			monitor.Verify(m => m.Initialize(It.Is<ApplicationSettings>(s => s == context.Settings.Applications)), Times.Once);
			monitor.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationTerminationFailedEventArgs));
		}

		[TestMethod]
		public void Perform_MustFailIfTerminationFails()
		{
			var application = new RunningApplication(default(string));
			var initialization = new InitializationResult();
			var args = new List<ActionRequiredEventArgs>();

			initialization.RunningApplications.Add(application);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(initialization);
			monitor.Setup(m => m.TryTerminate(It.IsAny<RunningApplication>())).Returns(false);
			sut.ActionRequired += (a) =>
			{
				args.Add(a);

				if (a is ApplicationTerminationEventArgs t)
				{
					t.TerminateProcesses = true;
				}
			};

			var result = sut.Perform();

			monitor.Verify(m => m.Initialize(It.Is<ApplicationSettings>(s => s == context.Settings.Applications)), Times.Once);
			monitor.Verify(m => m.TryTerminate(It.Is<RunningApplication>(a => a == application)), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsInstanceOfType(args[0], typeof(ApplicationTerminationEventArgs));
			Assert.IsInstanceOfType(args[1], typeof(ApplicationTerminationFailedEventArgs));
		}

		[TestMethod]
		public void Perform_MustIndicateApplicationInitializationFailure()
		{
			var application = new Mock<IApplication>().Object;
			var applicationSettings = new WhitelistApplication();
			var args = default(ActionRequiredEventArgs);

			context.Settings.Applications.Whitelist.Add(applicationSettings);
			factory.Setup(f => f.TryCreate(It.IsAny<WhitelistApplication>(), out application)).Returns(FactoryResult.Error);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());
			sut.ActionRequired += (a) => args = a;

			var result = sut.Perform();

			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == applicationSettings), out application), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationInitializationFailedEventArgs));
		}

		[TestMethod]
		public void Perform_MustInitializeApplications()
		{
			var application1 = new Mock<IApplication>().Object;
			var application2 = new Mock<IApplication>().Object;
			var application3 = new Mock<IApplication>().Object;
			var application1Settings = new WhitelistApplication();
			var application2Settings = new WhitelistApplication();
			var application3Settings = new WhitelistApplication();

			context.Settings.Applications.Whitelist.Add(application1Settings);
			context.Settings.Applications.Whitelist.Add(application2Settings);
			context.Settings.Applications.Whitelist.Add(application3Settings);
			factory.Setup(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application1Settings), out application1)).Returns(FactoryResult.Success);
			factory.Setup(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application2Settings), out application2)).Returns(FactoryResult.Success);
			factory.Setup(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application3Settings), out application3)).Returns(FactoryResult.Success);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());

			var result = sut.Perform();

			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application1Settings), out application1), Times.Once);
			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application1Settings), out application2), Times.Once);
			factory.Verify(f => f.TryCreate(It.Is<WhitelistApplication>(a => a == application1Settings), out application3), Times.Once);
			monitor.Verify(m => m.Initialize(It.Is<ApplicationSettings>(s => s == context.Settings.Applications)), Times.Once);
			monitor.Verify(m => m.TryTerminate(It.IsAny<RunningApplication>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotStartMonitorWithoutKioskMode()
		{
			context.Settings.Security.KioskMode = KioskMode.None;
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());

			var result = sut.Perform();

			monitor.Verify(m => m.Start(), Times.Never);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustStartMonitorWithKioskMode()
		{
			context.Settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());

			var result = sut.Perform();

			monitor.Verify(m => m.Start(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);

			context.Settings.Security.KioskMode = KioskMode.DisableExplorerShell;
			monitor.Reset();
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(new InitializationResult());

			result = sut.Perform();

			monitor.Verify(m => m.Start(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustTerminateRunningApplications()
		{
			var application1 = new RunningApplication(default(string));
			var application2 = new RunningApplication(default(string));
			var application3 = new RunningApplication(default(string));
			var initialization = new InitializationResult();
			var args = default(ActionRequiredEventArgs);

			initialization.RunningApplications.Add(application1);
			initialization.RunningApplications.Add(application2);
			initialization.RunningApplications.Add(application3);
			monitor.Setup(m => m.Initialize(It.IsAny<ApplicationSettings>())).Returns(initialization);
			monitor.Setup(m => m.TryTerminate(It.IsAny<RunningApplication>())).Returns(true);
			sut.ActionRequired += (a) =>
			{
				args = a;

				if (a is ApplicationTerminationEventArgs t)
				{
					t.TerminateProcesses = true;
				}
			};

			var result = sut.Perform();

			monitor.Verify(m => m.Initialize(It.Is<ApplicationSettings>(s => s == context.Settings.Applications)), Times.Once);
			monitor.Verify(m => m.TryTerminate(It.Is<RunningApplication>(a => a == application1)), Times.Once);
			monitor.Verify(m => m.TryTerminate(It.Is<RunningApplication>(a => a == application2)), Times.Once);
			monitor.Verify(m => m.TryTerminate(It.Is<RunningApplication>(a => a == application3)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsInstanceOfType(args, typeof(ApplicationTerminationEventArgs));
		}

		[TestMethod]
		public void Revert_MustNotStopMonitorWithoutKioskMode()
		{
			context.Settings.Security.KioskMode = KioskMode.None;

			var result = sut.Revert();

			monitor.Verify(m => m.Stop(), Times.Never);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustStopMonitorWithKioskMode()
		{
			context.Settings.Security.KioskMode = KioskMode.CreateNewDesktop;

			var result = sut.Revert();

			monitor.Verify(m => m.Stop(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);

			context.Settings.Security.KioskMode = KioskMode.DisableExplorerShell;
			monitor.Reset();

			result = sut.Revert();

			monitor.Verify(m => m.Stop(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustTerminateApplications()
		{
			var application1 = new Mock<IApplication>();
			var application2 = new Mock<IApplication>();
			var application3 = new Mock<IApplication>();

			context.Applications.Add(application1.Object);
			context.Applications.Add(application2.Object);
			context.Applications.Add(application3.Object);

			var result = sut.Revert();

			application1.Verify(a => a.Terminate(), Times.Once);
			application2.Verify(a => a.Terminate(), Times.Once);
			application3.Verify(a => a.Terminate(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
