/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Service.Operations;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class SessionInitializationOperationTests
	{
		private Mock<IAutoRestoreMechanism> autoRestoreMechanism;
		private Mock<ILogger> logger;
		private Mock<Func<string, EventWaitHandle>> serviceEventFactory;
		private SessionContext sessionContext;
		private SessionInitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			autoRestoreMechanism = new Mock<IAutoRestoreMechanism>();
			logger = new Mock<ILogger>();
			serviceEventFactory = new Mock<Func<string, EventWaitHandle>>();
			sessionContext = new SessionContext();

			serviceEventFactory.Setup(f => f.Invoke(It.IsAny<string>())).Returns(new EventStub());
			sessionContext.AutoRestoreMechanism = autoRestoreMechanism.Object;
			sessionContext.Configuration = new ServiceConfiguration
			{
				AppConfig = new AppConfig { ServiceEventName = $"{nameof(SafeExamBrowser)}-{nameof(SessionInitializationOperationTests)}" },
				Settings = new AppSettings()
			};

			sut = new SessionInitializationOperation(logger.Object, serviceEventFactory.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustCloseOldAndInitializeNewServiceEvent()
		{
			var stub = new EventStub();

			sessionContext.ServiceEvent = stub;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(stub.IsClosed);
			Assert.AreNotSame(stub, sessionContext.ServiceEvent);
			Assert.IsInstanceOfType(sessionContext.ServiceEvent, typeof(EventWaitHandle));
		}

		[TestMethod]
		public void Perform_MustStopAutoRestoreMechanism()
		{
			var result = sut.Perform();

			autoRestoreMechanism.Verify(m => m.Stop(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDeleteConfiguration()
		{
			sessionContext.Configuration = new ServiceConfiguration();
			sessionContext.ServiceEvent = new EventStub();

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsNull(sessionContext.Configuration);
		}

		[TestMethod]
		public void Revert_MustSetServiceEvent()
		{
			sessionContext.IsRunning = true;
			sessionContext.ServiceEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

			var wasSet = false;
			var task = Task.Run(() => wasSet = sessionContext.ServiceEvent.WaitOne(1000));
			var result = sut.Revert();

			task.Wait();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(wasSet);
		}

		[TestMethod]
		public void Revert_MustNotSetServiceEventIfNoSessionActive()
		{
			sessionContext.IsRunning = false;
			sessionContext.ServiceEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

			var wasSet = false;
			var task = Task.Run(() => wasSet = sessionContext.ServiceEvent.WaitOne(1000));
			var result = sut.Revert();

			task.Wait();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsFalse(wasSet);
		}

		[TestMethod]
		public void Revert_MustStartAutoRestoreMechanism()
		{
			var result = sut.Revert();

			autoRestoreMechanism.Verify(m => m.Start(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustResetSessionFlag()
		{
			sessionContext.IsRunning = true;

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsFalse(sessionContext.IsRunning);
		}
	}
}
