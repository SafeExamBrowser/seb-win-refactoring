/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class RuntimeConnectionOperationTests
	{
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<IRuntimeProxy> runtime;
		private Guid token;
		private RuntimeConnectionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			logger = new Mock<ILogger>();
			runtime = new Mock<IRuntimeProxy>();
			token = Guid.NewGuid();

			sut = new RuntimeConnectionOperation(context, logger.Object, runtime.Object, token);
		}

		[TestMethod]
		public void MustConnectOnPerform()
		{
			runtime.Setup(r => r.Connect(It.Is<Guid>(t => t == token), true)).Returns(true);

			var result = sut.Perform();

			runtime.Verify(r => r.Connect(It.Is<Guid>(t => t == token), true), Times.Once);
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustCorrectlyFailOnPerform()
		{
			runtime.Setup(r => r.Connect(It.Is<Guid>(t => t == token), true)).Returns(false);

			var result = sut.Perform();

			runtime.Verify(r => r.Connect(It.Is<Guid>(t => t == token), true), Times.Once);
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustDisconnectOnRevert()
		{
			runtime.Setup(r => r.Connect(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
			runtime.Setup(r => r.Disconnect()).Returns(true);
			runtime.SetupGet(r => r.IsConnected).Returns(true);
			sut.Perform();

			var result = sut.Revert();

			runtime.Verify(r => r.Connect(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
			runtime.Verify(r => r.Disconnect(), Times.Once);
			runtime.VerifyGet(r => r.IsConnected, Times.Once);
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustCorrectlyFailOnRevert()
		{
			runtime.Setup(r => r.Connect(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(true);
			runtime.Setup(r => r.Disconnect()).Returns(false);
			runtime.SetupGet(r => r.IsConnected).Returns(true);
			sut.Perform();

			var result = sut.Revert();

			runtime.Verify(r => r.Connect(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
			runtime.Verify(r => r.Disconnect(), Times.Once);
			runtime.VerifyGet(r => r.IsConnected, Times.Once);
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustDoNothingOnRevertIfNotConnected()
		{
			var result = sut.Revert();

			runtime.VerifyGet(r => r.IsConnected, Times.Once);
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
