/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class VirtualMachineOperationTests
	{
		private Mock<IVirtualMachineDetector> detector;
		private Mock<ILogger> logger;
		private SessionContext context;
		private VirtualMachineOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			detector = new Mock<IVirtualMachineDetector>();
			logger = new Mock<ILogger>();
			context = new SessionContext();

			context.Next = new SessionConfiguration();
			context.Next.Settings = new AppSettings();

			sut = new VirtualMachineOperation(detector.Object, logger.Object, context);
		}

		[TestMethod]
		public void Perform_MustAbortIfVirtualMachineNotAllowed()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;
			detector.Setup(d => d.IsVirtualMachine()).Returns(true);

			var result = sut.Perform();

			detector.Verify(d => d.IsVirtualMachine(), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfVirtualMachineAllowed()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			detector.Setup(d => d.IsVirtualMachine()).Returns(true);

			var result = sut.Perform();

			detector.Verify(d => d.IsVirtualMachine(), Times.AtMostOnce);
			Assert.AreEqual(OperationResult.Success, result);

			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			detector.Setup(d => d.IsVirtualMachine()).Returns(false);

			result = sut.Perform();

			detector.Verify(d => d.IsVirtualMachine(), Times.AtMost(2));
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfNotAVirtualMachine()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;
			detector.Setup(d => d.IsVirtualMachine()).Returns(false);

			var result = sut.Perform();

			detector.Verify(d => d.IsVirtualMachine(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustAbortIfVirtualMachineNotAllowed()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;
			detector.Setup(d => d.IsVirtualMachine()).Returns(true);

			var result = sut.Repeat();

			detector.Verify(d => d.IsVirtualMachine(), Times.Once);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedIfVirtualMachineAllowed()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			detector.Setup(d => d.IsVirtualMachine()).Returns(true);

			var result = sut.Repeat();

			detector.Verify(d => d.IsVirtualMachine(), Times.AtMostOnce);
			Assert.AreEqual(OperationResult.Success, result);

			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			detector.Setup(d => d.IsVirtualMachine()).Returns(false);

			result = sut.Repeat();

			detector.Verify(d => d.IsVirtualMachine(), Times.AtMost(2));
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedIfNotAVirtualMachine()
		{
			context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;
			detector.Setup(d => d.IsVirtualMachine()).Returns(false);

			var result = sut.Repeat();

			detector.Verify(d => d.IsVirtualMachine(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var result = sut.Revert();

			detector.VerifyNoOtherCalls();
			logger.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
