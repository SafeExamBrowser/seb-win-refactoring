/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RenderProcessMessageHandler : IRenderProcessMessageHandler
	{
		private readonly AppConfig appConfig;
		private readonly ContentLoader contentLoader;
		private readonly IKeyGenerator keyGenerator;

		internal RenderProcessMessageHandler(AppConfig appConfig, IKeyGenerator keyGenerator, IText text)
		{
			this.appConfig = appConfig;
			this.contentLoader = new ContentLoader(text);
			this.keyGenerator = keyGenerator;
		}

		public void OnContextCreated(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
		{
			var browserExamKey = keyGenerator.CalculateBrowserExamKeyHash(frame.Url);
			var configurationKey = keyGenerator.CalculateConfigurationKeyHash(frame.Url);
			var api = contentLoader.LoadApi(browserExamKey, configurationKey, appConfig.ProgramBuildVersion);

			frame.ExecuteJavaScriptAsync(api);
		}

		public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
		{
		}

		public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
		{
		}

		public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
		{
		}
	}
}
