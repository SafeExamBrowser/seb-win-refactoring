/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Service.Operations;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class ServiceEventCleanupOperationTests
	{
		private Mock<ILogger> logger;
		private SessionContext sessionContext;
		private ServiceEventCleanupOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sessionContext = new SessionContext();

			sut = new ServiceEventCleanupOperation(logger.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustDoNothing()
		{
			var serviceEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

			sessionContext.ServiceEvent = serviceEvent;

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(serviceEvent, sessionContext.ServiceEvent);
		}

		[TestMethod]
		public void Revert_MustCloseEvent()
		{
			var serviceEvent = new EventStub();

			sessionContext.ServiceEvent = serviceEvent;

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(serviceEvent.IsClosed);
		}

		[TestMethod]
		public void Revert_MustNotFailIfEventNull()
		{
			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
