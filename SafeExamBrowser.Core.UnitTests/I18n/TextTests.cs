/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Core.I18n;

namespace SafeExamBrowser.Core.UnitTests.I18n
{
	[TestClass]
	public class TextTests
	{
		[TestMethod]
		public void MustNeverReturnNull()
		{
			var resource = new Mock<ITextResource>();
			var sut = new Text(resource.Object);

			resource.Setup(r => r.LoadText()).Returns<IDictionary<Key, string>>(null);

			var text = sut.Get((Key)(-1));

			Assert.IsNotNull(text);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullResource()
		{
			new Text(null);
		}
	}
}
