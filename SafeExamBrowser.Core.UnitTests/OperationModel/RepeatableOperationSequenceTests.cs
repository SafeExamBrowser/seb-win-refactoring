/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeExamBrowser.Core.UnitTests.OperationModel
{
	[TestClass]
	public class RepeatableOperationSequenceTests
	{
		[TestInitialize]
		public void Initialize()
		{

		}

		[TestMethod]
		public void TODO()
		{
			Assert.Fail();
		}

		//[TestMethod]
		//public void MustCorrectlyAbortRepeat()
		//{
		//	var operationA = new Mock<IOperation>();
		//	var operationB = new Mock<IOperation>();
		//	var operationC = new Mock<IOperation>();
		//	var operations = new Queue<IOperation>();

		//	operationA.Setup(o => o.Repeat()).Returns(OperationResult.Success);
		//	operationB.Setup(o => o.Repeat()).Returns(OperationResult.Aborted);

		//	operations.Enqueue(operationA.Object);
		//	operations.Enqueue(operationB.Object);
		//	operations.Enqueue(operationC.Object);

		//	var sut = new OperationSequence(loggerMock.Object, operations);
		//	var result = sut.TryRepeat();

		//	operationA.Verify(o => o.Repeat(), Times.Once);
		//	operationA.Verify(o => o.Revert(), Times.Never);
		//	operationB.Verify(o => o.Repeat(), Times.Once);
		//	operationB.Verify(o => o.Revert(), Times.Never);
		//	operationC.Verify(o => o.Repeat(), Times.Never);
		//	operationC.Verify(o => o.Revert(), Times.Never);

		//	Assert.AreEqual(OperationResult.Aborted, result);
		//}

		//[TestMethod]
		//public void MustRepeatOperations()
		//{
		//	var operationA = new Mock<IOperation>();
		//	var operationB = new Mock<IOperation>();
		//	var operationC = new Mock<IOperation>();
		//	var operations = new Queue<IOperation>();

		//	operationA.Setup(o => o.Repeat()).Returns(OperationResult.Success);
		//	operationB.Setup(o => o.Repeat()).Returns(OperationResult.Success);
		//	operationC.Setup(o => o.Repeat()).Returns(OperationResult.Success);

		//	operations.Enqueue(operationA.Object);
		//	operations.Enqueue(operationB.Object);
		//	operations.Enqueue(operationC.Object);

		//	var sut = new OperationSequence(loggerMock.Object, operations);
		//	var result = sut.TryRepeat();

		//	operationA.Verify(o => o.Perform(), Times.Never);
		//	operationA.Verify(o => o.Repeat(), Times.Once);
		//	operationA.Verify(o => o.Revert(), Times.Never);
		//	operationB.Verify(o => o.Perform(), Times.Never);
		//	operationB.Verify(o => o.Repeat(), Times.Once);
		//	operationB.Verify(o => o.Revert(), Times.Never);
		//	operationC.Verify(o => o.Perform(), Times.Never);
		//	operationC.Verify(o => o.Repeat(), Times.Once);
		//	operationC.Verify(o => o.Revert(), Times.Never);

		//	Assert.AreEqual(OperationResult.Success, result);
		//}

		//[TestMethod]
		//public void MustRepeatOperationsInSequence()
		//{
		//	int current = 0, a = 0, b = 0, c = 0;
		//	var operationA = new Mock<IOperation>();
		//	var operationB = new Mock<IOperation>();
		//	var operationC = new Mock<IOperation>();
		//	var operations = new Queue<IOperation>();

		//	operationA.Setup(o => o.Repeat()).Returns(OperationResult.Success).Callback(() => a = ++current);
		//	operationB.Setup(o => o.Repeat()).Returns(OperationResult.Success).Callback(() => b = ++current);
		//	operationC.Setup(o => o.Repeat()).Returns(OperationResult.Success).Callback(() => c = ++current);

		//	operations.Enqueue(operationA.Object);
		//	operations.Enqueue(operationB.Object);
		//	operations.Enqueue(operationC.Object);

		//	var sut = new OperationSequence(loggerMock.Object, operations);
		//	var result = sut.TryRepeat();

		//	Assert.AreEqual(OperationResult.Success, result);
		//	Assert.IsTrue(a == 1);
		//	Assert.IsTrue(b == 2);
		//	Assert.IsTrue(c == 3);
		//}

		//[TestMethod]
		//public void MustNotRevertOperationsInCaseOfError()
		//{
		//	var operationA = new Mock<IOperation>();
		//	var operationB = new Mock<IOperation>();
		//	var operationC = new Mock<IOperation>();
		//	var operationD = new Mock<IOperation>();
		//	var operations = new Queue<IOperation>();

		//	operationA.Setup(o => o.Repeat()).Returns(OperationResult.Success);
		//	operationB.Setup(o => o.Repeat()).Returns(OperationResult.Success);
		//	operationC.Setup(o => o.Repeat()).Throws<Exception>();

		//	operations.Enqueue(operationA.Object);
		//	operations.Enqueue(operationB.Object);
		//	operations.Enqueue(operationC.Object);
		//	operations.Enqueue(operationD.Object);

		//	var sut = new OperationSequence(loggerMock.Object, operations);
		//	var result = sut.TryRepeat();

		//	operationA.Verify(o => o.Repeat(), Times.Once);
		//	operationA.Verify(o => o.Revert(), Times.Never);
		//	operationB.Verify(o => o.Repeat(), Times.Once);
		//	operationB.Verify(o => o.Revert(), Times.Never);
		//	operationC.Verify(o => o.Repeat(), Times.Once);
		//	operationC.Verify(o => o.Revert(), Times.Never);
		//	operationD.Verify(o => o.Repeat(), Times.Never);
		//	operationD.Verify(o => o.Revert(), Times.Never);

		//	Assert.AreEqual(OperationResult.Failed, result);
		//}

		//[TestMethod]
		//public void MustSucceedRepeatingWithEmptyQueue()
		//{
		//	var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());
		//	var result = sut.TryRepeat();

		//	Assert.AreEqual(OperationResult.Success, result);
		//}

		//[TestMethod]
		//public void MustSucceedRepeatingWithoutCallingPerform()
		//{
		//	var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());
		//	var result = sut.TryRepeat();

		//	Assert.AreEqual(OperationResult.Success, result);
		//}

		//[TestMethod]
		//public void MustNotFailInCaseOfUnexpectedErrorWhenRepeating()
		//{
		//	var sut = new OperationSequence(loggerMock.Object, new Queue<IOperation>());

		//	sut.ProgressChanged += (args) => throw new Exception();

		//	var result = sut.TryRepeat();

		//	Assert.AreEqual(OperationResult.Failed, result);
		//}
	}
}
