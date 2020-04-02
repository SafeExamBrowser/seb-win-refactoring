/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class ContextMenuHandlerTests
	{
		private ContextMenuHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new ContextMenuHandler();
		}

		[TestMethod]
		public void MustClearContextMenu()
		{
			var menu = new Mock<IMenuModel>();

			sut.OnBeforeContextMenu(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IContextMenuParams>(), menu.Object);
			menu.Verify(m => m.Clear(), Times.Once);
		}

		[TestMethod]
		public void MustBlockContextMenu()
		{
			var command = sut.OnContextMenuCommand(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IContextMenuParams>(), default(CefMenuCommand), default(CefEventFlags));
			var run = sut.RunContextMenu(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IContextMenuParams>(), Mock.Of<IMenuModel>(), Mock.Of<IRunContextMenuCallback>());

			Assert.IsFalse(command);
			Assert.IsFalse(run);
		}
	}
}
