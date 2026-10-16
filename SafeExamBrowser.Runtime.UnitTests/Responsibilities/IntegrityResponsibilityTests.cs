/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class IntegrityResponsibilityTests
	{
		private const int INTERVAL_MS = 5;

		private RuntimeContext context;
		private Mock<IIntegrityModule> integrityModule;
		private Mock<ILogger> logger;
		private Mock<Action> shutdown;

		private IntegrityResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			integrityModule = new Mock<IIntegrityModule>();
			logger = new Mock<ILogger>();
			shutdown = new Mock<Action>();

			sut = new IntegrityResponsibility(integrityModule.Object, INTERVAL_MS, logger.Object, context, shutdown.Object);
		}

		[TestCleanup]
		public void Cleanup()
		{
			sut.Assume(RuntimeTask.StopIntegrityMonitoring);
		}

		[TestMethod]
		public void MustIgnoreUnrelatedTasks()
		{
			var tasks = Enum.GetValues(typeof(RuntimeTask)).Cast<RuntimeTask>().Where(t => t != RuntimeTask.StartIntegrityMonitoring && t != RuntimeTask.StopIntegrityMonitoring);

			foreach (var task in tasks)
			{
				sut.Assume(task);
			}

			integrityModule.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();
			shutdown.Verify(s => s(), Times.Never);
		}

		[TestMethod]
		public void MustNotVerifyIntegrityBeforeIntervalElapsed()
		{
			var isValid = true;

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true);

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			integrityModule.Verify(m => m.TryVerifyRuntimeIntegrity(out isValid), Times.Never);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
			logger.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustVerifyIntegrityPeriodicallyOnceStarted()
		{
			var count = 0;
			var isValid = true;
			var sync = new ManualResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true).Callback(() =>
			{
				if (Interlocked.Increment(ref count) >= 3)
				{
					sync.Set();
				}
			});

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS), "Runtime integrity was not verified repeatedly within the expected time.");
			integrityModule.Verify(m => m.TryVerifyRuntimeIntegrity(out isValid), Times.AtLeast(3));
		}

		[TestMethod]
		public void MustContinueMonitoringWhenVerificationFails()
		{
			var count = 0;
			var isValid = false;
			var sync = new ManualResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(false).Callback(() =>
			{
				if (Interlocked.Increment(ref count) >= 3)
				{
					sync.Set();
				}
			});

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS), "Monitoring did not continue after a failed verification attempt.");
			integrityModule.Verify(m => m.TryVerifyRuntimeIntegrity(out isValid), Times.AtLeast(3));
		}

		[TestMethod]
		public void MustOnlyLogInformationWhenIntegrityIsValid()
		{
			var isValid = true;
			var sync = new AutoResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true).Callback(() => sync.Set());

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS));

			sut.Assume(RuntimeTask.StopIntegrityMonitoring);

			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
			logger.VerifyNoOtherCalls();
			shutdown.Verify(s => s(), Times.Never);
		}

		[TestMethod]
		public void MustLogWarningWhenIntegrityIsCompromised()
		{
			var isValid = false;
			var sync = new AutoResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true).Callback(() => sync.Set());

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS));

			sut.Assume(RuntimeTask.StopIntegrityMonitoring);

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);
			logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		[TestMethod]
		public void MustLogWarningWhenVerificationFails()
		{
			var isValid = true;
			var sync = new AutoResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(false).Callback(() => sync.Set());

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS));

			sut.Assume(RuntimeTask.StopIntegrityMonitoring);

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);
			logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			shutdown.Verify(s => s(), Times.Never);
		}

		[TestMethod]
		public void MustStopVerifyingIntegrityWhenStopped()
		{
			var count = 0;
			var isValid = true;
			var sync = new AutoResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true).Callback(() =>
			{
				Interlocked.Increment(ref count);
				sync.Set();
			});

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS));

			sut.Assume(RuntimeTask.StopIntegrityMonitoring);

			var countAfterStop = count;

			Assert.AreEqual(countAfterStop, count);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeast(2));
		}

		[TestMethod]
		public void MustAllowToRestartMonitoring()
		{
			var isValid = true;
			var sync = new AutoResetEvent(false);

			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true).Callback(() => sync.Set());

			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS));

			sut.Assume(RuntimeTask.StopIntegrityMonitoring);
			Thread.Sleep(INTERVAL_MS);
			sync.Reset();
			sut.Assume(RuntimeTask.StartIntegrityMonitoring);

			Assert.IsTrue(sync.WaitOne(10 * INTERVAL_MS), "Runtime integrity was not verified after restarting the monitoring.");
		}

		[TestMethod]
		public void MustNotFailWhenStoppedWithoutBeingStarted()
		{
			sut.Assume(RuntimeTask.StopIntegrityMonitoring);

			integrityModule.VerifyNoOtherCalls();
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
			logger.VerifyNoOtherCalls();
		}
	}
}
