/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Browser.Content;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RenderProcessMessageHandler : IRenderProcessMessageHandler
	{
		private readonly AppConfig appConfig;
		private readonly ContentLoader contentLoader;
		private readonly ILogger logger;
		private readonly IKeyGenerator generator;

		internal RenderProcessMessageHandler(AppConfig appConfig, ILogger logger, IKeyGenerator generator, IText text)
		{
			this.appConfig = appConfig;
			this.contentLoader = new ContentLoader(text);
			this.logger = logger;
			this.generator = generator;
		}

		public void OnContextCreated(IWebBrowser webBrowser, IBrowser browser, IFrame frame)
		{
			var browserExamKey = generator.CalculateBrowserExamKeyHash(webBrowser.Address);
			var configurationKey = generator.CalculateConfigurationKeyHash(webBrowser.Address);
			var api = contentLoader.LoadApi(browserExamKey, configurationKey, appConfig.ProgramBuildVersion);

			frame.ExecuteJavaScriptAsync(api);
		}

		public void OnContextReleased(IWebBrowser webBrowser, IBrowser browser, IFrame frame)
		{
		}

		public void OnFocusedNodeChanged(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IDomNode node)
		{
		}

		public void OnUncaughtException(IWebBrowser webBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
		{
		}
	}
}
