/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Core.OperationModel;

namespace SafeExamBrowser.Core.UnitTests.OperationModel
{
	[TestClass]
	public class QueueExtensionTests
	{
		[TestMethod]
		public void MustCorrectlyIterateThroughQueue()
		{
			var order = 0;
			var queue = new Queue<int>(Enumerable.Range(1, 25));
			var action = new Action<int>(i =>
			{
				Assert.AreEqual(i, ++order);
				Assert.AreEqual(queue.ElementAt(i - 1), i);
			});

			queue.ForEach(action);

			Assert.AreEqual(queue.Count, order);
		}
	}
}
