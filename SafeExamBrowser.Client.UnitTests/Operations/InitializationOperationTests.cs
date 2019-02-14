/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class InitializationOperationTests
	{
		private Mock<ILogger> logger;
		private InitializationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new InitializationOperation(logger.Object);
		}

		[TestMethod]
		public void MustPerformSuccessfully()
		{
			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
			Assert.IsTrue(ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls));
			Assert.IsTrue(ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls11));
			Assert.IsTrue(ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12));
			Assert.IsTrue(ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Ssl3));
		}

		[TestMethod]
		public void MustRevertSuccessfully()
		{
			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
