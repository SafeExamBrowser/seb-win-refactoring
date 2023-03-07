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
using SafeExamBrowser.Monitoring.Contracts.Keyboard;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class KeyboardInterceptorOperationTests
	{
		private ClientContext context;
		private Mock<IKeyboardInterceptor> keyboardInterceptorMock;
		private Mock<ILogger> loggerMock;

		private KeyboardInterceptorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			keyboardInterceptorMock = new Mock<IKeyboardInterceptor>();
			loggerMock = new Mock<ILogger>();

			sut = new KeyboardInterceptorOperation(context, keyboardInterceptorMock.Object, loggerMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			keyboardInterceptorMock.Verify(i => i.Start(), Times.Once);
			keyboardInterceptorMock.Verify(i => i.Stop(), Times.Never);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			keyboardInterceptorMock.Verify(i => i.Start(), Times.Never);
			keyboardInterceptorMock.Verify(i => i.Stop(), Times.Once);
		}
	}
}
