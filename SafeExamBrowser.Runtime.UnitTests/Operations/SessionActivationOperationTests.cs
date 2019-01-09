/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class SessionActivationOperationTests
	{
		private Mock<ISessionConfiguration> currentSession;
		private Mock<ILogger> logger;
		private Mock<ISessionConfiguration> nextSession;
		private SessionContext sessionContext;

		private SessionActivationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new Mock<ISessionConfiguration>();
			logger = new Mock<ILogger>();
			nextSession = new Mock<ISessionConfiguration>();
			sessionContext = new SessionContext();

			sessionContext.Current = currentSession.Object;
			sessionContext.Next = nextSession.Object;

			sut = new SessionActivationOperation(logger.Object, sessionContext);
		}

		[TestMethod]
		public void MustCorrectlyActivateFirstSession()
		{
			sessionContext.Current = null;

			var result = sut.Perform();

			currentSession.VerifyNoOtherCalls();
			nextSession.VerifyGet(s => s.Id);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(sessionContext.Current, nextSession.Object);
			Assert.IsNull(sessionContext.Next);
		}

		[TestMethod]
		public void MustCorrectlySwitchSession()
		{
			var result = sut.Repeat();

			currentSession.VerifyGet(s => s.Id);
			nextSession.VerifyGet(s => s.Id);

			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreSame(sessionContext.Current, nextSession.Object);
			Assert.IsNull(sessionContext.Next);
		}

		[TestMethod]
		public void MustAlwaysRevertSuccessfully()
		{
			var result = sut.Revert();

			currentSession.VerifyNoOtherCalls();
			nextSession.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
