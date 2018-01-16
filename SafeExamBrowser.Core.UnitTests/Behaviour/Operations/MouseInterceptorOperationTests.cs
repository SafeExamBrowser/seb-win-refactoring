/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
	public class MouseInterceptorOperationTests
	{
		private Mock<IMouseInterceptor> mouseInterceptorMock;
		private Mock<ILogger> loggerMock;
		private Mock<INativeMethods> nativeMethodsMock;
		private Mock<ISplashScreen> splashScreenMock;

		private MouseInterceptorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			mouseInterceptorMock = new Mock<IMouseInterceptor>();
			loggerMock = new Mock<ILogger>();
			nativeMethodsMock = new Mock<INativeMethods>();
			splashScreenMock = new Mock<ISplashScreen>();

			sut = new MouseInterceptorOperation(loggerMock.Object, mouseInterceptorMock.Object, nativeMethodsMock.Object)
			{
				SplashScreen = splashScreenMock.Object
			};
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			nativeMethodsMock.Verify(n => n.RegisterMouseHook(It.IsAny<IMouseInterceptor>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			nativeMethodsMock.Verify(n => n.DeregisterMouseHook(It.IsAny<IMouseInterceptor>()), Times.Once);
		}
	}
}
