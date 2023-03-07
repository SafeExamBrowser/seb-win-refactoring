/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class SystemMonitorOperationTests
	{
		private ClientContext context;
		private Mock<ISystemMonitor> systemMonitor;
		private Mock<ILogger> logger;
		private SystemMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			systemMonitor = new Mock<ISystemMonitor>();
			logger = new Mock<ILogger>();

			sut = new SystemMonitorOperation(context, systemMonitor.Object, logger.Object);
		}

		[TestMethod]
		public void Perform_MustStartMonitor()
		{
			sut.Perform();

			systemMonitor.Verify(s => s.Start(), Times.Once);
			systemMonitor.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void Revert_MustStopMonitor()
		{
			sut.Revert();

			systemMonitor.Verify(s => s.Stop(), Times.Once);
			systemMonitor.VerifyNoOtherCalls();
		}
	}
}
