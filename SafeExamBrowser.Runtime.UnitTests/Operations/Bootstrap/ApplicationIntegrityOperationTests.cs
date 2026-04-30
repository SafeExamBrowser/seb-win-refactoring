/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Bootstrap;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Bootstrap
{
	[TestClass]
	public class ApplicationIntegrityOperationTests
	{
		private Mock<IIntegrityModule> integrityModule;
		private Mock<ILogger> logger;
		private ApplicationIntegrityOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			integrityModule = new Mock<IIntegrityModule>();
			logger = new Mock<ILogger>();

			sut = new ApplicationIntegrityOperation(integrityModule.Object, logger.Object);
		}

		[TestMethod]
		public void Perform_MustVerifyCodeSignature()
		{
			var isValid = true;

			integrityModule.Setup(m => m.TryVerifyCodeSignature(out isValid)).Returns(true);
			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true);

			var result = sut.Perform();

			integrityModule.Verify(m => m.TryVerifyCodeSignature(out isValid), Times.Once);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustLogCompromise()
		{
			var isValid = false;

			integrityModule.Setup(m => m.TryVerifyCodeSignature(out isValid)).Returns(true);
			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(true);

			var result = sut.Perform();

			integrityModule.Verify(m => m.TryVerifyCodeSignature(out isValid), Times.Once);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustLogFailure()
		{
			var isValid = true;

			integrityModule.Setup(m => m.TryVerifyCodeSignature(out isValid)).Returns(false);
			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out isValid)).Returns(false);

			var result = sut.Perform();

			integrityModule.Verify(m => m.TryVerifyCodeSignature(out isValid), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Exactly(2));

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustContinueOnCodeSignatureCompromise()
		{
			var codeSignature = false;
			var runtimeIntegrity = true;

			integrityModule.Setup(m => m.TryVerifyCodeSignature(out codeSignature)).Returns(true);
			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out runtimeIntegrity)).Returns(true);

			var result = sut.Perform();

			integrityModule.Verify(m => m.TryVerifyCodeSignature(out codeSignature), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustTerminateOnRuntimeIntegrityCompromise()
		{
			var codeSignature = true;
			var runtimeIntegrity = false;

			integrityModule.Setup(m => m.TryVerifyCodeSignature(out codeSignature)).Returns(true);
			integrityModule.Setup(m => m.TryVerifyRuntimeIntegrity(out runtimeIntegrity)).Returns(true);

			var result = sut.Perform();

			integrityModule.Verify(m => m.TryVerifyCodeSignature(out codeSignature), Times.Once);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var result = sut.Revert();

			integrityModule.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
