/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Service.UnitTests
{
	[TestClass]
	public class ServiceControllerTests
	{
		private Mock<ILogger> logger;
		private Mock<Func<string, ILogObserver>> logWriterFactory;
		private Mock<IOperationSequence> bootstrapSequence;
		private SessionContext sessionContext;
		private Mock<IOperationSequence> sessionSequence;
		private Mock<IServiceHost> serviceHost;
		private Mock<ISystemConfigurationUpdate> systemConfigurationUpdate;
		private ServiceController sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			logWriterFactory = new Mock<Func<string, ILogObserver>>();
			bootstrapSequence = new Mock<IOperationSequence>();
			sessionContext = new SessionContext();
			sessionSequence = new Mock<IOperationSequence>();
			serviceHost = new Mock<IServiceHost>();
			systemConfigurationUpdate = new Mock<ISystemConfigurationUpdate>();

			logWriterFactory.Setup(f => f.Invoke(It.IsAny<string>())).Returns(new Mock<ILogObserver>().Object);

			sut = new ServiceController(
				logger.Object,
				logWriterFactory.Object,
				bootstrapSequence.Object,
				sessionSequence.Object,
				serviceHost.Object,
				sessionContext,
				systemConfigurationUpdate.Object);
		}

		[TestMethod]
		public void Communication_MustStartNewSessionUponRequest()
		{
			var args = new SessionStartEventArgs { Configuration = new ServiceConfiguration { SessionId = Guid.NewGuid() } };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStartRequested += null, args);

			sessionSequence.Verify(s => s.TryPerform(), Times.Once);

			Assert.IsNotNull(sessionContext.Configuration);
			Assert.AreEqual(args.Configuration.SessionId, sessionContext.Configuration.SessionId);
		}

		[TestMethod]
		public void Communication_MustInitializeSessionLogging()
		{
			var args = new SessionStartEventArgs
			{
				Configuration = new ServiceConfiguration
				{
					AppConfig = new AppConfig { ServiceLogFilePath = "Test.log" },
					SessionId = Guid.NewGuid(),
					Settings = new AppSettings { LogLevel = LogLevel.Warning }
				}
			};

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStartRequested += null, args);

			logger.VerifySet(l => l.LogLevel = It.Is<LogLevel>(ll => ll == args.Configuration.Settings.LogLevel), Times.Once);
		}

		[TestMethod]
		public void Communication_MustNotAllowNewSessionDuringActiveSession()
		{
			var args = new SessionStartEventArgs { Configuration = new ServiceConfiguration { SessionId = Guid.NewGuid() } };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.Configuration = new ServiceConfiguration { SessionId = Guid.NewGuid() };
			sessionContext.IsRunning = true;

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStartRequested += null, args);

			sessionSequence.Verify(s => s.TryPerform(), Times.Never);

			Assert.AreNotEqual(args.Configuration.SessionId, sessionContext.Configuration.SessionId);
		}

		[TestMethod]
		public void Communication_MustNotFailIfNoValidSessionData()
		{
			var args = new SessionStartEventArgs { Configuration = null };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStartRequested += null, args);

			sessionSequence.Verify(s => s.TryPerform(), Times.Once);

			Assert.IsNull(sessionContext.Configuration);
		}

		[TestMethod]
		public void Communication_MustStopActiveSessionUponRequest()
		{
			var args = new SessionStopEventArgs { SessionId = Guid.NewGuid() };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.Configuration = new ServiceConfiguration { SessionId = args.SessionId };
			sessionContext.IsRunning = true;

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStopRequested += null, args);

			sessionSequence.Verify(s => s.TryRevert(), Times.Once);
		}

		[TestMethod]
		public void Communication_MustNotStopSessionWithWrongId()
		{
			var args = new SessionStopEventArgs { SessionId = Guid.NewGuid() };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.Configuration = new ServiceConfiguration { SessionId = Guid.NewGuid() };
			sessionContext.IsRunning = true;

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStopRequested += null, args);

			sessionSequence.Verify(s => s.TryRevert(), Times.Never);
		}

		[TestMethod]
		public void Communication_MustNotStopSessionWithoutActiveSession()
		{
			var args = new SessionStopEventArgs { SessionId = Guid.NewGuid() };

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.IsRunning = false;

			sut.TryStart();
			serviceHost.Raise(h => h.SessionStopRequested += null, args);

			sessionSequence.Verify(s => s.TryRevert(), Times.Never);
		}

		[TestMethod]
		public void Communication_MustStartSystemConfigurationUpdate()
		{
			var sync = new AutoResetEvent(false);

			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			systemConfigurationUpdate.Setup(u => u.ExecuteAsync()).Callback(() => sync.Set());

			sut.TryStart();
			serviceHost.Raise(h => h.SystemConfigurationUpdateRequested += null);

			sync.WaitOne();

			systemConfigurationUpdate.Verify(u => u.Execute(), Times.Never);
			systemConfigurationUpdate.Verify(u => u.ExecuteAsync(), Times.Once);
		}

		[TestMethod]
		public void Start_MustOnlyPerformBootstrapSequence()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.Configuration = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.IsTrue(success);
		}

		[TestMethod]
		public void Stop_MustRevertSessionThenBootstrapSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);
			sessionContext.Configuration = new ServiceConfiguration();
			sessionContext.IsRunning = true;

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Once);

			Assert.AreEqual(1, session);
			Assert.AreEqual(2, bootstrap);
		}

		[TestMethod]
		public void Stop_MustNotRevertSessionSequenceIfNoSessionRunning()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);
			sessionContext.Configuration = null;

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.AreEqual(0, session);
			Assert.AreEqual(1, bootstrap);
		}
	}
}
