/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.Behaviour.Operations;

namespace SafeExamBrowser.Core.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class KeyboardInterceptorOperationTests
	{
		private Mock<IKeyboardInterceptor> keyboardInterceptorMock;
		private Mock<ILogger> loggerMock;
		private Mock<INativeMethods> nativeMethodsMock;
		private Mock<ISplashScreen> splashScreenMock;

		private KeyboardInterceptorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			keyboardInterceptorMock = new Mock<IKeyboardInterceptor>();
			loggerMock = new Mock<ILogger>();
			nativeMethodsMock = new Mock<INativeMethods>();
			splashScreenMock = new Mock<ISplashScreen>();

			sut = new KeyboardInterceptorOperation(keyboardInterceptorMock.Object, loggerMock.Object, nativeMethodsMock.Object)
			{
				SplashScreen = splashScreenMock.Object
			};
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
