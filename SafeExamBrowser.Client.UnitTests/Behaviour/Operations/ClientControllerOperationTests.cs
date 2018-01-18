/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Behaviour.Operations;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ClientControllerOperationTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IClientController> clientControllerMock;
		private Mock<ISplashScreen> splashScreenMock;

		private ClientControllerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			clientControllerMock = new Mock<IClientController>();
			splashScreenMock = new Mock<ISplashScreen>();

			sut = new ClientControllerOperation(clientControllerMock.Object, loggerMock.Object)
			{
				SplashScreen = splashScreenMock.Object
			};
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();

			clientControllerMock.Verify(r => r.Start(), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			clientControllerMock.Verify(r => r.Stop(), Times.Once);
		}
	}
}
