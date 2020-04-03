/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class LifeSpanHandlerTests
	{
		private LifeSpanHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new LifeSpanHandler();
		}

		[TestMethod]
		public void MustUseDefaultBehavior()
		{
			Assert.IsFalse(sut.DoClose(default(IWebBrowser), default(IBrowser)));
		}

		[TestMethod]
		public void MustHandlePopup()
		{
			var args = default(PopupRequestedEventArgs);
			var jsAccess = false;
			var url = "https://www.host.org/some-url";

			sut.PopupRequested += (a) => args = a;

			var result = sut.OnBeforePopup(default(IWebBrowser), default(IBrowser), default(IFrame), url, default(string), default(WindowOpenDisposition), default(bool), default(IPopupFeatures), default(IWindowInfo), default(IBrowserSettings), ref jsAccess, out var newBrowser);

			Assert.IsTrue(result);
			Assert.AreEqual(default(IWebBrowser), newBrowser);
			Assert.AreEqual(url, args.Url);
		}
	}
}
