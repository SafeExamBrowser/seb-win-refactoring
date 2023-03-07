/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Configuration.Cryptography;

namespace SafeExamBrowser.Configuration.UnitTests.Cryptography
{
	[TestClass]
	public class HashAlgorithmTests
	{
		private HashAlgorithm sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new HashAlgorithm();
		}

		[TestMethod]
		public void MustGeneratePasswordHashCorrectly()
		{
			var hash = "4adfa806cb610693a6200e4cdbdafeaf352876a35f964a781d691457df9cd378";
			var generated = sut.GenerateHashFor("blabbedyblubbedy");

			Assert.AreEqual(hash, generated);
		}
	}
}
