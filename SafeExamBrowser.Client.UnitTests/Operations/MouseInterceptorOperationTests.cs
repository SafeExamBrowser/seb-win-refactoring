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
using SafeExamBrowser.Monitoring.Contracts.Mouse;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class MouseInterceptorOperationTests
	{
		private ClientContext context;
		private Mock<IMouseInterceptor> mouseInterceptorMock;
		private Mock<ILogger> loggerMock;

		private MouseInterceptorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			mouseInterceptorMock = new Mock<IMouseInterceptor>();
			loggerMock = new Mock<ILogger>();

			sut = new MouseInterceptorOperation(context, loggerMock.Object, mouseInterceptorMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			mouseInterceptorMock.Verify(i => i.Start(), Times.Once);
			mouseInterceptorMock.Verify(i => i.Stop(), Times.Never);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			mouseInterceptorMock.Verify(i => i.Start(), Times.Never);
			mouseInterceptorMock.Verify(i => i.Stop(), Times.Once);
		}
	}
}
