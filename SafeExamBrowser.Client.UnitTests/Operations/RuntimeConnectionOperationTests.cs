/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class RuntimeConnectionOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IRuntimeProxy> runtime;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			runtime = new Mock<IRuntimeProxy>();
		}

		[TestMethod]
		public void TODO()
		{
			Assert.Fail();
		}
	}
}
