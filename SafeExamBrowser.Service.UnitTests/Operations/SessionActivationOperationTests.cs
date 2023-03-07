/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Service.Operations;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class SessionActivationOperationTests
	{
		private Mock<ILogger> logger;
		private SessionContext sessionContext;
		private SessionActivationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sessionContext = new SessionContext();

			sut = new SessionActivationOperation(logger.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustSetServiceEvent()
		{
			sessionContext.ServiceEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

			var wasSet = false;
			var task = Task.Run(() => wasSet = sessionContext.ServiceEvent.WaitOne(1000));
			var result = sut.Perform();

			task.Wait();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(wasSet);
			Assert.IsTrue(sessionContext.IsRunning);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			sessionContext.IsRunning = true;

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(sessionContext.IsRunning);
		}
	}
}
