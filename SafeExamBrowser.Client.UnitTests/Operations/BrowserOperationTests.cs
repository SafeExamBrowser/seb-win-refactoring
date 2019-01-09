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
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class BrowserOperationTests
	{
		private Mock<IApplicationController> controllerMock;
		private Mock<IApplicationInfo> appInfoMock;
		private Mock<ILogger> loggerMock;
		private Mock<ITaskbar> taskbarMock;
		private Mock<IUserInterfaceFactory> uiFactoryMock;

		private BrowserOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			controllerMock = new Mock<IApplicationController>();
			appInfoMock = new Mock<IApplicationInfo>();
			loggerMock = new Mock<ILogger>();
			taskbarMock = new Mock<ITaskbar>();
			uiFactoryMock = new Mock<IUserInterfaceFactory>();

			sut = new BrowserOperation(controllerMock.Object, appInfoMock.Object, loggerMock.Object, taskbarMock.Object, uiFactoryMock.Object);
		}

		[TestMethod]
		public void MustPeformCorrectly()
		{
			var order = 0;

			controllerMock.Setup(c => c.Initialize()).Callback(() => Assert.AreEqual(++order, 1));
			controllerMock.Setup(c => c.RegisterApplicationButton(It.IsAny<IApplicationButton>())).Callback(() => Assert.AreEqual(++order, 2));
			taskbarMock.Setup(t => t.AddApplication(It.IsAny<IApplicationButton>())).Callback(() => Assert.AreEqual(++order, 3));

			sut.Perform();

			controllerMock.Verify(c => c.Initialize(), Times.Once);
			controllerMock.Verify(c => c.RegisterApplicationButton(It.IsAny<IApplicationButton>()), Times.Once);
			taskbarMock.Verify(t => t.AddApplication(It.IsAny<IApplicationButton>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();

			controllerMock.Verify(c => c.Terminate(), Times.Once);
		}
	}
}
