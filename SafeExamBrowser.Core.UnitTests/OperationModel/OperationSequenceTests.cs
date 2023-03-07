/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Core.OperationModel;

namespace SafeExamBrowser.Core.UnitTests.OperationModel
{
	[TestClass]
	public class OperationSequenceTests
	{
		private Mock<ILogger> loggerMock;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
		}

		[TestMethod]
		public void MustCreateCopyOfOperationQueue()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			operations.Clear();

			sut.TryPerform();

			operationA.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Once);
		}

		[TestMethod]
		public void MustCorrectlyPropagateEventSubscription()
		{
			var actionRequiredCalled = false;
			var actionRequiredHandler = new ActionRequiredEventHandler(args => actionRequiredCalled = true);
			var statusChangedCalled = false;
			var statusChangedHandler = new StatusChangedEventHandler(t => statusChangedCalled = true);
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success).Raises(o => o.ActionRequired += null, new Mock<ActionRequiredEventArgs>().Object);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success).Raises(o => o.StatusChanged += null, default(TextKey));

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			sut.ActionRequired += actionRequiredHandler;
			sut.StatusChanged += statusChangedHandler;

			sut.TryPerform();

			Assert.IsTrue(actionRequiredCalled);
			Assert.IsTrue(statusChangedCalled);

			actionRequiredCalled = false;
			statusChangedCalled = false;
			sut.ActionRequired -= actionRequiredHandler;
			sut.StatusChanged -= statusChangedHandler;

			sut.TryPerform();

			Assert.IsFalse(actionRequiredCalled);
			Assert.IsFalse(statusChangedCalled);
		}

		#region Perform Tests

		[TestMethod]
		public void MustCorrectlyAbortPerform()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Aborted);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var result = sut.TryPerform();

			operationA.Verify(o => o.Perform(), Times.Once);
			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Never);
			operationC.Verify(o => o.Revert(), Times.Never);

			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void MustPerformOperations()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var result = sut.TryPerform();

			operationA.Verify(o => o.Perform(), Times.Once);
			operationA.Verify(o => o.Revert(), Times.Never);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Never);
			operationC.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustPerformOperationsInSequence()
		{
			int current = 0, a = 0, b = 0, c = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success).Callback(() => a = ++current);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success).Callback(() => b = ++current);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success).Callback(() => c = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var result = sut.TryPerform();

			Assert.AreEqual(OperationResult.Success, result);
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

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Throws<Exception>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);
			operations.Enqueue(operationD.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var result = sut.TryPerform();

			operationA.Verify(o => o.Perform(), Times.Once);
			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Perform(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Perform(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);
			operationD.Verify(o => o.Perform(), Times.Never);
			operationD.Verify(o => o.Revert(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustRevertOperationsInSequenceAfterPerformError()
		{
			int current = 0, a = 0, b = 0, c = 0, d = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operationD = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Revert()).Callback(() => a = ++current);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Revert()).Callback(() => b = ++current);
			operationC.Setup(o => o.Perform()).Throws<Exception>();
			operationC.Setup(o => o.Revert()).Callback(() => c = ++current);
			operationD.Setup(o => o.Revert()).Callback(() => d = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);
			operations.Enqueue(operationD.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var result = sut.TryPerform();

			Assert.AreEqual(OperationResult.Failed, result);
			Assert.IsTrue(d == 0);
			Assert.IsTrue(c == 1);
			Assert.IsTrue(b == 2);
			Assert.IsTrue(a == 3);
		}

		[TestMethod]
		public void MustContinueToRevertOperationsAfterPerformError()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Throws<Exception>();

			operationC.Setup(o => o.Revert()).Throws<Exception>();
			operationB.Setup(o => o.Revert()).Throws<Exception>();
			operationA.Setup(o => o.Revert()).Throws<Exception>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);
			var success = sut.TryPerform();

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
			var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());
			var result = sut.TryPerform();

			Assert.AreEqual(OperationResult.Success, result);
		}


		[TestMethod]
		public void MustNotFailInCaseOfUnexpectedError()
		{
			var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());

			sut.ProgressChanged += (args) => throw new Exception();

			var result = sut.TryPerform();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		#endregion

		#region Revert Tests

		[TestMethod]
		public void MustRevertOperations()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Revert()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Revert()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Revert()).Returns(OperationResult.Success);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			sut.TryPerform();

			var result = sut.TryRevert();

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustRevertOperationsInSequence()
		{
			int current = 0, a = 0, b = 0, c = 0;
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success);

			operationA.Setup(o => o.Revert()).Returns(OperationResult.Success).Callback(() => a = ++current);
			operationB.Setup(o => o.Revert()).Returns(OperationResult.Success).Callback(() => b = ++current);
			operationC.Setup(o => o.Revert()).Returns(OperationResult.Success).Callback(() => c = ++current);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			sut.TryPerform();

			var result = sut.TryRevert();

			Assert.AreEqual(OperationResult.Success, result);
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

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationC.Setup(o => o.Perform()).Returns(OperationResult.Success);

			operationA.Setup(o => o.Revert()).Throws<Exception>();
			operationB.Setup(o => o.Revert()).Throws<Exception>();
			operationC.Setup(o => o.Revert()).Throws<Exception>();

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			sut.TryPerform();

			var result = sut.TryRevert();

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustOnlyRevertPerformedOperations()
		{
			var operationA = new Mock<IOperation>();
			var operationB = new Mock<IOperation>();
			var operationC = new Mock<IOperation>();
			var operations = new Queue<IOperation>();

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Revert()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Aborted);
			operationB.Setup(o => o.Revert()).Returns(OperationResult.Success);

			operations.Enqueue(operationA.Object);
			operations.Enqueue(operationB.Object);
			operations.Enqueue(operationC.Object);

			var sut = new OperationSequence(loggerMock.Object, operations);

			sut.TryPerform();

			var result = sut.TryRevert();

			operationA.Verify(o => o.Revert(), Times.Once);
			operationB.Verify(o => o.Revert(), Times.Once);
			operationC.Verify(o => o.Revert(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustSucceedWithEmptyQueueWhenReverting()
		{
			var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());

			sut.TryPerform();

			var result = sut.TryRevert();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustSucceedRevertingWithoutCallingPerform()
		{
			var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());
			var result = sut.TryRevert();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustNotFailInCaseOfUnexpectedErrorWhenReverting()
		{
			var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());

			sut.ProgressChanged += (args) => throw new Exception();

			var result = sut.TryRevert();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		#endregion
	}
}
