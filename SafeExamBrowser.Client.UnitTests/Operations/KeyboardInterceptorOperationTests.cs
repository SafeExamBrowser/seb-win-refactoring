/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class KeyboardInterceptorOperationTests
	{
		private Mock<IKeyboardInterceptor> keyboardInterceptorMock;
		private Mock<ILogger> loggerMock;
		private Mock<INativeMethods> nativeMethodsMock;

		private KeyboardInterceptorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			keyboardInterceptorMock = new Mock<IKeyboardInterceptor>();
			loggerMock = new Mock<ILogger>();
			nativeMethodsMock = new Mock<INativeMethods>();

			sut = new KeyboardInterceptorOperation(keyboardInterceptorMock.Object, loggerMock.Object, nativeMethodsMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			nativeMethodsMock.Verify(n => n.RegisterKeyboardHook(It.IsAny<IKeyboardInterceptor>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			nativeMethodsMock.Verify(n => n.DeregisterKeyboardHook(It.IsAny<IKeyboardInterceptor>()), Times.Once);
		}
	}
}
