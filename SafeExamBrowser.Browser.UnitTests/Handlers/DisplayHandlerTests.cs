/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class DisplayHandlerTests
	{
		private DisplayHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new DisplayHandler();
		}

		[TestMethod]
		public void MustUseDefaultHandling()
		{
			var text = default(string);

			Assert.IsFalse(sut.OnAutoResize(default, default, default));
			Assert.IsFalse(sut.OnConsoleMessage(default, default));
			Assert.IsFalse(sut.OnCursorChange(default, default, default, default, default));
			Assert.IsFalse(sut.OnTooltipChanged(default, ref text));
		}

		[TestMethod]
		public void MustHandleFaviconChange()
		{
			var newUrl = "www.someurl.org/favicon.ico";
			var url = default(string);
			var called = false;

			sut.FaviconChanged += (u) =>
			{
				called = true;
				url = u;
			};
			sut.OnFaviconUrlChange(default, default, new List<string>());

			Assert.AreEqual(default, url);
			Assert.IsFalse(called);

			sut.OnFaviconUrlChange(default, default, new List<string> { newUrl });

			Assert.AreEqual(newUrl, url);
			Assert.IsTrue(called);
		}

		[TestMethod]
		public void MustHandleProgressChange()
		{
			var expected = 0.123456;
			var actual = default(double);

			sut.ProgressChanged += (p) => actual = p;
			sut.OnLoadingProgressChange(default, default, expected);

			Assert.AreEqual(expected, actual);
		}
	}
}
