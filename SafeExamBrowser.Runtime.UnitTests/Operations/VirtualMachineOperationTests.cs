/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class VirtualMachineOperationTests
	{
		private RuntimeContext context;
		private Mock<IVirtualMachineDetector> detector;
		private Mock<ILogger> logger;

		private VirtualMachineOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			detector = new Mock<IVirtualMachineDetector>();
			logger = new Mock<ILogger>();

			context.Next = new SessionConfiguration();
			context.Next.Settings = new AppSettings();

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), context),
				logger.Object,
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				context,
				Mock.Of<IText>());

			sut = new VirtualMachineOperation(dependencies, detector.Object);
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
