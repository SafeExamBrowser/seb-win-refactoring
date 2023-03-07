/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Operations;

namespace SafeExamBrowser.Core.UnitTests.Operations
{
	[TestClass]
	public class DelegateOperationTests
	{
		[TestMethod]
		public void MustExecutePerformAction()
		{
			var performed = false;
			void perform() => performed = true;
			var sut = new DelegateOperation(perform);

			sut.Perform();

			Assert.IsTrue(performed);
		}

		[TestMethod]
		public void MustExecuteRepeatAction()
		{
			var repeated = false;
			void repeat() => repeated = true;
			var sut = new DelegateOperation(() => { }, repeat);

			sut.Repeat();

			Assert.IsTrue(repeated);
		}

		[TestMethod]
		public void MustExecuteRevertAction()
		{
			var reverted = false;
			void revert() => reverted = true;
			var sut = new DelegateOperation(() => { }, revert: revert);

			sut.Revert();

			Assert.IsTrue(reverted);
		}

		[TestMethod]
		public void MustNotFailIfActionsAreNull()
		{
			var sut = new DelegateOperation(null, null, null);

			var perform = sut.Perform();
			var repeat = sut.Repeat();
			var revert = sut.Revert();

			Assert.AreEqual(OperationResult.Success, perform);
			Assert.AreEqual(OperationResult.Success, repeat);
			Assert.AreEqual(OperationResult.Success, revert);
		}
	}
}
