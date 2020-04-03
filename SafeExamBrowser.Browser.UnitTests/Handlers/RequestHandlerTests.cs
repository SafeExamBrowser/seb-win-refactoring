/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class RequestHandlerTests
	{
		private AppConfig appConfig;
		private Mock<IRequestFilter> filter;
		private Mock<ILogger> logger;
		private BrowserSettings settings;
		private Mock<IText> text;
		private RequestHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			filter = new Mock<IRequestFilter>();
			logger = new Mock<ILogger>();
			settings = new BrowserSettings();
			text = new Mock<IText>();

			sut = new RequestHandler(appConfig, filter.Object, logger.Object, settings, text.Object);
		}

		[TestMethod]
		public void TODO()
		{
			// Use inheritance to test functionality!
		}
	}
}
