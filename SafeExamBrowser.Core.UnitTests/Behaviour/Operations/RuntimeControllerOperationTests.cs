/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour.Operations;

namespace SafeExamBrowser.Core.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class RuntimeControllerOperationTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IRuntimeController> runtimeControllerMock;
		private Mock<ISplashScreen> splashScreenMock;

		private RuntimeControllerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			runtimeControllerMock = new Mock<IRuntimeController>();
			splashScreenMock = new Mock<ISplashScreen>();

			sut = new RuntimeControllerOperation(runtimeControllerMock.Object, loggerMock.Object)
			{
				SplashScreen = splashScreenMock.Object
			};
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			runtimeControllerMock.Verify(r => r.Start(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			runtimeControllerMock.Verify(r => r.Stop(), Times.Once);
		}
	}
}
