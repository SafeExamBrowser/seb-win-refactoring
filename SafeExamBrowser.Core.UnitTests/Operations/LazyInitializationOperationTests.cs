/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Operations;

namespace SafeExamBrowser.Core.UnitTests.Operations
{
	[TestClass]
	public class LazyInitializationOperationTests
	{
		private Mock<IOperation> operationMock;

		[TestInitialize]
		public void Initialize()
		{
			operationMock = new Mock<IOperation>();
		}

		[TestMethod]
		public void MustInstantiateOperationOnPerform()
		{
			var initialized = false;
			IOperation initialize()
			{
				initialized = true;

				return operationMock.Object;
			};

			var sut = new LazyInitializationOperation(initialize);

			sut.Perform();

			Assert.IsTrue(initialized);
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void MustNotInstantiateOperationOnRepeat()
		{
			IOperation initialize()
			{
				return operationMock.Object;
			};

			var sut = new LazyInitializationOperation(initialize);

			sut.Repeat();
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void MustNotInstantiateOperationOnRevert()
		{
			IOperation initialize()
			{
				return operationMock.Object;
			};

			var sut = new LazyInitializationOperation(initialize);

			sut.Revert();
		}

		[TestMethod]
		public void MustReturnCorrectOperationResult()
		{
			IOperation initialize()
			{
				return operationMock.Object;
			};

			operationMock.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationMock.Setup(o => o.Repeat()).Returns(OperationResult.Failed);

			var sut = new LazyInitializationOperation(initialize);
			var perform = sut.Perform();
			var repeat = sut.Repeat();

			sut.Revert();

			Assert.AreEqual(OperationResult.Success, perform);
			Assert.AreEqual(OperationResult.Failed, repeat);
		}

		[TestMethod]
		public void MustUpdateProgressIndicator()
		{
			IOperation initialize()
			{
				return operationMock.Object;
			};

			var sut = new LazyInitializationOperation(initialize)
			{
				ProgressIndicator = new Mock<IProgressIndicator>().Object
			};

			sut.Perform();
			sut.Repeat();
			sut.Revert();

			operationMock.VerifySet(o => o.ProgressIndicator = It.IsAny<IProgressIndicator>(), Times.Exactly(3));
		}

		[TestMethod]
		public void MustUseSameInstanceForAllOperations()
		{
			var first = true;
			var operation = operationMock.Object;
			IOperation initialize()
			{
				if (first)
				{
					return operation;
				}

				return new Mock<IOperation>().Object;
			};

			var sut = new LazyInitializationOperation(initialize);

			sut.Perform();
			sut.Repeat();
			sut.Revert();

			operationMock.Verify(o => o.Perform(), Times.Once);
			operationMock.Verify(o => o.Repeat(), Times.Once);
			operationMock.Verify(o => o.Revert(), Times.Once);
		}
	}
}
