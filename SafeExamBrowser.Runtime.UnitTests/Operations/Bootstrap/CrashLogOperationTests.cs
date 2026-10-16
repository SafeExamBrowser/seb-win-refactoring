/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Bootstrap;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Bootstrap
{
	[TestClass]
	public class CrashLogOperationTests
	{
		private AppConfig appConfig;
		private RuntimeContext context;
		private Mock<ICrashLog> crashLog;
		private Mock<ILogger> logger;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private CrashLogOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			context = new RuntimeContext();
			crashLog = new Mock<ICrashLog>();
			logger = new Mock<ILogger>();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();

			appConfig.BrowserLogFilePath = @"C:\Logs\Browser.log";
			appConfig.ClientLogFilePath = @"C:\Logs\Client.log";
			appConfig.RuntimeLogFilePath = @"C:\Logs\Runtime.log";
			appConfig.ServiceLogFilePath = @"C:\Logs\Service.log";
			context.Responsibilities = responsibilities.Object;

			sut = new CrashLogOperation(appConfig, crashLog.Object, logger.Object, context);
		}

		[TestMethod]
		public void Perform_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Perform();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_InitializeCrashLog, key);
		}

		[TestMethod]
		public void Perform_MustStoreCurrentLogFilePaths()
		{
			crashLog.SetupGet(c => c.HasData).Returns(false);

			var result = sut.Perform();

			crashLog.Verify(c => c.Set(appConfig.BrowserLogFilePath, appConfig.ClientLogFilePath, appConfig.RuntimeLogFilePath, appConfig.ServiceLogFilePath), Times.Once);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotShowCrashMessageWithoutCrashData()
		{
			crashLog.SetupGet(c => c.HasData).Returns(false);

			var result = sut.Perform();

			crashLog.Verify(c => c.Get(), Times.Never);
			crashLog.Verify(c => c.Clear(), Times.Never);
			responsibilities.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustShowCrashMessageWithCrashData()
		{
			var data = new[] { @"C:\Logs\Previous_Browser.log", @"C:\Logs\Previous_Client.log" };

			crashLog.SetupGet(c => c.HasData).Returns(true);
			crashLog.Setup(c => c.Get()).Returns(data);
			responsibilities
				.Setup(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.IsAny<string[]>()))
				.Returns(new ErrorDialogResult());

			var result = sut.Perform();

			crashLog.Verify(c => c.Get(), Times.Once);
			responsibilities.Verify(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.Is<string[]>(d => d.SequenceEqual(data))), Times.Once);
			responsibilities.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustClearCrashDataWhenRequested()
		{
			var order = 0;
			var clear = 0;
			var delegation = 0;
			var set = 0;

			crashLog.SetupGet(c => c.HasData).Returns(true);
			crashLog.Setup(c => c.Get()).Returns(new[] { @"C:\Logs\Previous_Browser.log" });
			crashLog.Setup(c => c.Clear()).Callback(() => clear = ++order);
			crashLog.Setup(c => c.Set(It.IsAny<string[]>())).Callback(() => set = ++order);
			responsibilities
				.Setup(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.IsAny<string[]>()))
				.Returns(new ErrorDialogResult { ClearCrash = true })
				.Callback(() => delegation = ++order);

			var result = sut.Perform();

			crashLog.Verify(c => c.Clear(), Times.Once);
			crashLog.Verify(c => c.Set(appConfig.BrowserLogFilePath, appConfig.ClientLogFilePath, appConfig.RuntimeLogFilePath, appConfig.ServiceLogFilePath), Times.Once);

			Assert.AreEqual(1, delegation);
			Assert.AreEqual(2, clear);
			Assert.AreEqual(3, set);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustKeepCrashDataWhenNotRequested()
		{
			crashLog.SetupGet(c => c.HasData).Returns(true);
			crashLog.Setup(c => c.Get()).Returns(new[] { @"C:\Logs\Previous_Browser.log" });
			responsibilities
				.Setup(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.IsAny<string[]>()))
				.Returns(new ErrorDialogResult { ClearCrash = false });

			var result = sut.Perform();

			crashLog.Verify(c => c.Clear(), Times.Never);
			crashLog.Verify(c => c.Set(appConfig.BrowserLogFilePath, appConfig.ClientLogFilePath, appConfig.RuntimeLogFilePath, appConfig.ServiceLogFilePath), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Revert();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_FinalizeCrashLog, key);
		}

		[TestMethod]
		public void Revert_MustClearCrashLog()
		{
			var result = sut.Revert();

			crashLog.Verify(c => c.Clear(), Times.Once);
			crashLog.Verify(c => c.Set(It.IsAny<string[]>()), Times.Never);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
			responsibilities.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustNotRestoreCrashDataWithoutPreviousCrash()
		{
			crashLog.SetupGet(c => c.HasData).Returns(false);

			sut.Perform();
			crashLog.Invocations.Clear();

			var result = sut.Revert();

			crashLog.Verify(c => c.Clear(), Times.Once);
			crashLog.Verify(c => c.Set(It.IsAny<string[]>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustNotRestoreCrashDataWhenCleared()
		{
			crashLog.SetupGet(c => c.HasData).Returns(true);
			crashLog.Setup(c => c.Get()).Returns(new[] { @"C:\Logs\Previous_Browser.log", @"C:\Logs\Previous_Client.log" });
			responsibilities
				.Setup(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.IsAny<string[]>()))
				.Returns(new ErrorDialogResult { ClearCrash = true });

			sut.Perform();
			crashLog.Invocations.Clear();

			var result = sut.Revert();

			crashLog.Verify(c => c.Clear(), Times.Once);
			crashLog.Verify(c => c.Set(It.IsAny<string[]>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustRestoreCrashDataWhenKept()
		{
			var data = new[] { @"C:\Logs\Previous_Browser.log", @"C:\Logs\Previous_Client.log" };
			var order = 0;
			var clear = 0;
			var set = 0;

			crashLog.SetupGet(c => c.HasData).Returns(true);
			crashLog.Setup(c => c.Get()).Returns(data);
			responsibilities
				.Setup(r => r.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, It.IsAny<string[]>()))
				.Returns(new ErrorDialogResult { ClearCrash = false });

			sut.Perform();
			crashLog.Invocations.Clear();
			crashLog.Setup(c => c.Clear()).Callback(() => clear = ++order);
			crashLog.Setup(c => c.Set(It.IsAny<string[]>())).Callback(() => set = ++order);

			var result = sut.Revert();

			crashLog.Verify(c => c.Clear(), Times.Once);
			crashLog.Verify(c => c.Set(It.Is<string[]>(d => d.SequenceEqual(data))), Times.Once);
			crashLog.Verify(c => c.Set(It.IsAny<string[]>()), Times.Once);

			Assert.AreEqual(1, clear);
			Assert.AreEqual(2, set);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
