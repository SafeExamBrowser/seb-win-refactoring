/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Service.Operations;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class SessionInitializationOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<Func<string, ILogObserver>> logWriterFactory;
		private Mock<IServiceHost> serviceHost;
		private Mock<Func<string, EventWaitHandle>> serviceEventFactory;
		private SessionContext sessionContext;
		private SessionInitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			logWriterFactory = new Mock<Func<string, ILogObserver>>();
			serviceHost = new Mock<IServiceHost>();
			serviceEventFactory = new Mock<Func<string, EventWaitHandle>>();
			sessionContext = new SessionContext();

			logWriterFactory.Setup(f => f.Invoke(It.IsAny<string>())).Returns(new Mock<ILogObserver>().Object);
			serviceEventFactory.Setup(f => f.Invoke(It.IsAny<string>())).Returns(new EventStub());
			sessionContext.Configuration = new ServiceConfiguration
			{
				AppConfig = new AppConfig { ServiceEventName = $"{nameof(SafeExamBrowser)}-{nameof(SessionInitializationOperationTests)}" }
			};

			sut = new SessionInitializationOperation(logger.Object, logWriterFactory.Object, serviceEventFactory.Object, serviceHost.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustDisableNewConnections()
		{
			var result = sut.Perform();

			serviceHost.VerifySet(h => h.AllowConnection = false, Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
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
		public void Revert_MustSetServiceEvent()
		{
			sessionContext.ServiceEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

			var wasSet = false;
			var task = Task.Run(() => wasSet = sessionContext.ServiceEvent.WaitOne(1000));
			var result = sut.Revert();

			task.Wait();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(wasSet);
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
		public void Revert_MustEnableNewConnections()
		{
			sessionContext.ServiceEvent = new EventStub();

			var result = sut.Revert();

			serviceHost.VerifySet(h => h.AllowConnection = true, Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
