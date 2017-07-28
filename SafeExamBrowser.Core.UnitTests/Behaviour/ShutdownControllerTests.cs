/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour;

namespace SafeExamBrowser.Core.UnitTests.Behaviour
{
	[TestClass]
	public class ShutdownControllerTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<ISettings> settingsMock;
		private Mock<IText> textMock;
		private Mock<IUserInterfaceFactory> uiFactoryMock;

		private IShutdownController sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			settingsMock = new Mock<ISettings>();
			textMock = new Mock<IText>();
			uiFactoryMock = new Mock<IUserInterfaceFactory>();

			uiFactoryMock.Setup(f => f.CreateSplashScreen(settingsMock.Object, textMock.Object)).Returns(new Mock<ISplashScreen>().Object);

			sut = new ShutdownController(loggerMock.Object, settingsMock.Object, textMock.Object, uiFactoryMock.Object);
		}

		[TestMethod]
		public void MustRevertOperations()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			sut.FinalizeApplication(operations);

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);
		}

		[TestMethod]
		public void MustRevertOperationsInSequence()
		{
			int current = 0, a = 0, b = 0, c = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Revert()).Callback(() => a = ++current);
			operationB.Setup(o => o.Revert()).Callback(() => b = ++current);
			operationC.Setup(o => o.Revert()).Callback(() => c = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			sut.FinalizeApplication(operations);

			Assert.IsTrue(a == 1);
			Assert.IsTrue(b == 2);
			Assert.IsTrue(c == 3);
		}

		[TestMethod]
		public void MustNotFailWithEmptyQueue()
		{
			sut.FinalizeApplication(new Queue<IOperation>());
		}
	}
}
