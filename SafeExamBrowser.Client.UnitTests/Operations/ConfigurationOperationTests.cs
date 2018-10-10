/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private ClientConfiguration configuration;
		private Mock<ILogger> logger;
		private Mock<IRuntimeProxy> runtime;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			configuration = new ClientConfiguration();
			logger = new Mock<ILogger>();
			runtime = new Mock<IRuntimeProxy>();

			sut = new ConfigurationOperation(configuration, logger.Object, runtime.Object);
		}

		[TestMethod]
		public void TODO()
		{
			Assert.Fail();
		}
	}
}
