/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class CoordinatorTests
	{
		private Coordinator sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new Coordinator();
		}

		[TestMethod]
		public void ReconfigurationLock_MustWorkCorrectly()
		{
			Assert.IsFalse(sut.IsReconfigurationLocked());

			sut.RequestReconfigurationLock();

			var result = Parallel.For(1, 1000, (_) =>
			{
				Assert.IsTrue(sut.IsReconfigurationLocked());
				Assert.IsFalse(sut.RequestReconfigurationLock());
			});

			Assert.IsTrue(result.IsCompleted);

			result = Parallel.For(1, 1000, (_) =>
			{
				sut.ReleaseReconfigurationLock();
			});

			Assert.IsFalse(sut.IsReconfigurationLocked());
			Assert.IsTrue(result.IsCompleted);
		}

		[TestMethod]
		public void RequestReconfigurationLock_MustOnlyAllowLockingOnce()
		{
			var count = 0;

			Assert.IsFalse(sut.IsReconfigurationLocked());

			var result = Parallel.For(1, 1000, (_) =>
			{
				var acquired = sut.RequestReconfigurationLock();

				if (acquired)
				{
					Interlocked.Increment(ref count);
				}
			});

			Assert.AreEqual(1, count);
			Assert.IsTrue(sut.IsReconfigurationLocked());
			Assert.IsTrue(result.IsCompleted);
		}

		[TestMethod]
		public void RequestSessionLock_MustOnlyAllowLockingOnce()
		{
			var count = 0;

			Assert.IsFalse(sut.IsSessionLocked());

			var result = Parallel.For(1, 1000, (_) =>
			{
				var acquired = sut.RequestSessionLock();

				if (acquired)
				{
					Interlocked.Increment(ref count);
				}
			});

			Assert.AreEqual(1, count);
			Assert.IsTrue(sut.IsSessionLocked());
			Assert.IsTrue(result.IsCompleted);
		}

		[TestMethod]
		public void SessionLock_MustWorkCorrectly()
		{
			Assert.IsFalse(sut.IsSessionLocked());

			sut.RequestSessionLock();

			var result = Parallel.For(1, 1000, (_) =>
			{
				Assert.IsTrue(sut.IsSessionLocked());
				Assert.IsFalse(sut.RequestSessionLock());
			});

			Assert.IsTrue(result.IsCompleted);

			result = Parallel.For(1, 1000, (_) =>
			{
				sut.ReleaseSessionLock();
			});

			Assert.IsFalse(sut.IsSessionLocked());
			Assert.IsTrue(result.IsCompleted);
		}
	}
}
