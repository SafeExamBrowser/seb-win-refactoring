/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour;

namespace SafeExamBrowser.Core.UnitTests.Behaviour
{
	[TestClass]
	public class StartupControllerTests
	{
		private Mock<ILogger> loggerMock;
		private Mock<IRuntimeInfo> runtimeInfoMock;
		private Mock<ISystemInfo> systemInfoMock;
		private Mock<IText> textMock;
		private Mock<IUserInterfaceFactory> uiFactoryMock;

		private IStartupController sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			runtimeInfoMock = new Mock<IRuntimeInfo>();
			systemInfoMock = new Mock<ISystemInfo>();
			textMock = new Mock<IText>();
			uiFactoryMock = new Mock<IUserInterfaceFactory>();

			uiFactoryMock.Setup(f => f.CreateSplashScreen(runtimeInfoMock.Object, textMock.Object)).Returns(new Mock<ISplashScreen>().Object);

			sut = new StartupController(loggerMock.Object, runtimeInfoMock.Object, systemInfoMock.Object, textMock.Object, uiFactoryMock.Object);
		}

		[TestMethod]
		public void MustPerformOperations()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var result = sut.TryInitializeApplication(operations);

			operationA.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Once);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void MustPerformOperationsInSequence()
		{
			int current = 0, a = 0, b = 0, c = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Callback(() => a = ++current);
			operationB.Setup(o => o.Perform()).Callback(() => b = ++current);
			operationC.Setup(o => o.Perform()).Callback(() => c = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			sut.TryInitializeApplication(operations);

			Assert.IsTrue(a == 1);
			Assert.IsTrue(b == 2);
			Assert.IsTrue(c == 3);
		}

		[TestMethod]
		public void MustRevertOperationsInCaseOfError()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operationD = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationC.Setup(o => o.Perform()).Throws<Exception>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);
			operations.Enqueue(operationD.Object);

			var result = sut.TryInitializeApplication(operations);

			operationA.Verify(o => o.Perform(), Times.Once);
			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);
			operationD.Verify(o => o.Perform(), Times.Never);
			operationD.Verify(o => o.Revert(), Times.Never);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public void MustRevertOperationsInSequence()
		{
			int current = 0, a = 0, b = 0, c = 0, d = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operationD = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Revert()).Callback(() => a = ++current);
			operationB.Setup(o => o.Revert()).Callback(() => b = ++current);
			operationC.Setup(o => o.Revert()).Callback(() => c = ++current);
			operationC.Setup(o => o.Perform()).Throws<Exception>();
			operationD.Setup(o => o.Revert()).Callback(() => d = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);
			operations.Enqueue(operationD.Object);

			sut.TryInitializeApplication(operations);

			Assert.IsTrue(d == 0);
			Assert.IsTrue(c == 1);
			Assert.IsTrue(b == 2);
			Assert.IsTrue(a == 3);
		}

		[TestMethod]
		public void MustContinueToRevertOperationsInCaseOfError()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationC.Setup(o => o.Perform()).Throws<Exception>();
			operationC.Setup(o => o.Revert()).Throws<Exception>();
			operationB.Setup(o => o.Revert()).Throws<Exception>();
			operationA.Setup(o => o.Revert()).Throws<Exception>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var result = sut.TryInitializeApplication(operations);

			operationA.Verify(o => o.Perform(), Times.Once);
			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);
		}

		[TestMethod]
		public void MustSucceedWithEmptyQueue()
		{
			var result = sut.TryInitializeApplication(new Queue<IOperation>());

			Assert.IsTrue(result);
		}
	}
}
