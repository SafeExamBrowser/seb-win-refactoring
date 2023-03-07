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
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
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
			operationMock.Setup(o => o.Revert()).Returns(OperationResult.Failed);

			var sut = new LazyInitializationOperation(initialize);
			var perform = sut.Perform();
			var revert = sut.Revert();

			Assert.AreEqual(OperationResult.Success, perform);
			Assert.AreEqual(OperationResult.Failed, revert);
		}

		[TestMethod]
		public void MustCorrectlyHandleEventSubscription()
		{
			IOperation initialize()
			{
				return operationMock.Object;
			};

			var actionRequired = 0;
			var actionRequiredHandler = new ActionRequiredEventHandler(args => actionRequired++);
			var statusChanged = 0;
			var statusChangedHandler = new StatusChangedEventHandler(t => statusChanged++);
			var sut = new LazyInitializationOperation(initialize);

			sut.ActionRequired += actionRequiredHandler;
			sut.StatusChanged += statusChangedHandler;

			sut.Perform();

			operationMock.Raise(o => o.ActionRequired += null, new Mock<ActionRequiredEventArgs>().Object);
			operationMock.Raise(o => o.StatusChanged += null, default(TextKey));

			Assert.AreEqual(1, actionRequired);
			Assert.AreEqual(1, statusChanged);

			sut.ActionRequired -= actionRequiredHandler;
			sut.StatusChanged -= statusChangedHandler;

			operationMock.Raise(o => o.ActionRequired += null, new Mock<ActionRequiredEventArgs>().Object);
			operationMock.Raise(o => o.StatusChanged += null, default(TextKey));

			Assert.AreEqual(1, actionRequired);
			Assert.AreEqual(1, statusChanged);

			sut.ActionRequired += actionRequiredHandler;
			sut.StatusChanged += statusChangedHandler;

			operationMock.Raise(o => o.ActionRequired += null, new Mock<ActionRequiredEventArgs>().Object);
			operationMock.Raise(o => o.StatusChanged += null, default(TextKey));

			Assert.AreEqual(2, actionRequired);
			Assert.AreEqual(2, statusChanged);
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
					first = false;

					return operation;
				}

				return new Mock<IOperation>().Object;
			};

			var sut = new LazyInitializationOperation(initialize);

			sut.Perform();
			sut.Revert();

			operationMock.Verify(o => o.Perform(), Times.Once);
			operationMock.Verify(o => o.Revert(), Times.Once);
		}
	}
}
