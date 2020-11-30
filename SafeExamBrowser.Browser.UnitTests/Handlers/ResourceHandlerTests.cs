/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Net.Mime;
using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;
using ResourceHandler = SafeExamBrowser.Browser.Handlers.ResourceHandler;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class ResourceHandlerTests
	{
		private AppConfig appConfig;
		private Mock<IRequestFilter> filter;
		private Mock<ILogger> logger;
		private BrowserSettings settings;
		private WindowSettings windowSettings;
		private Mock<IText> text;
		private TestableResourceHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			filter = new Mock<IRequestFilter>();
			logger = new Mock<ILogger>();
			settings = new BrowserSettings();
			windowSettings = new WindowSettings();
			text = new Mock<IText>();

			sut = new TestableResourceHandler(appConfig, filter.Object, logger.Object, settings, windowSettings, text.Object);
		}

		[TestMethod]
		public void MustAppendCustomHeadersForSameDomain()
		{
			var browser = new Mock<IWebBrowser>();
			var headers = default(NameValueCollection);
			var request = new Mock<IRequest>();

			browser.SetupGet(b => b.Address).Returns("http://www.host.org");
			request.SetupGet(r => r.Headers).Returns(new NameValueCollection());
			request.SetupGet(r => r.Url).Returns("http://www.host.org");
			request.SetupSet(r => r.Headers = It.IsAny<NameValueCollection>()).Callback<NameValueCollection>((h) => headers = h);
			settings.SendConfigurationKey = true;
			settings.SendExamKey = true;

			var result = sut.OnBeforeResourceLoad(browser.Object, Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, Mock.Of<IRequestCallback>());

			request.VerifyGet(r => r.Headers, Times.AtLeastOnce);
			request.VerifySet(r => r.Headers = It.IsAny<NameValueCollection>(), Times.AtLeastOnce);

			Assert.AreEqual(CefReturnValue.Continue, result);
			Assert.IsNotNull(headers["X-SafeExamBrowser-ConfigKeyHash"]);
			Assert.IsNotNull(headers["X-SafeExamBrowser-RequestHash"]);
		}

		[TestMethod]
		public void MustNotAppendCustomHeadersForCrossDomain()
		{
			var browser = new Mock<IWebBrowser>();
			var headers = new NameValueCollection();
			var request = new Mock<IRequest>();

			browser.SetupGet(b => b.Address).Returns("http://www.otherhost.org");
			request.SetupGet(r => r.Headers).Returns(new NameValueCollection());
			request.SetupGet(r => r.Url).Returns("http://www.host.org");
			request.SetupSet(r => r.Headers = It.IsAny<NameValueCollection>()).Callback<NameValueCollection>((h) => headers = h);
			settings.SendConfigurationKey = true;
			settings.SendExamKey = true;

			var result = sut.OnBeforeResourceLoad(browser.Object, Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, Mock.Of<IRequestCallback>());

			request.VerifyGet(r => r.Headers, Times.Never);
			request.VerifySet(r => r.Headers = It.IsAny<NameValueCollection>(), Times.Never);

			Assert.AreEqual(CefReturnValue.Continue, result);
			Assert.IsNull(headers["X-SafeExamBrowser-ConfigKeyHash"]);
			Assert.IsNull(headers["X-SafeExamBrowser-RequestHash"]);
		}

		[TestMethod]
		public void MustBlockMailToUrls()
		{
			var request = new Mock<IRequest>();
			var url = $"{Uri.UriSchemeMailto}:someone@somewhere.org";

			request.SetupGet(r => r.Url).Returns(url);

			var result = sut.OnBeforeResourceLoad(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, Mock.Of<IRequestCallback>());

			Assert.AreEqual(CefReturnValue.Cancel, result);
		}

		[TestMethod]
		public void MustFilterContentRequests()
		{
			var request = new Mock<IRequest>();
			var url = "www.test.org";

			filter.Setup(f => f.Process(It.Is<Request>(r => r.Url.Equals(url)))).Returns(FilterResult.Block);
			request.SetupGet(r => r.ResourceType).Returns(ResourceType.SubFrame);
			request.SetupGet(r => r.Url).Returns(url);
			settings.Filter.ProcessContentRequests = true;
			settings.Filter.ProcessMainRequests = true;

			var resourceHandler = sut.GetResourceHandler(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object);

			filter.Verify(f => f.Process(It.Is<Request>(r => r.Url.Equals(url))), Times.Once);

			Assert.IsNotNull(resourceHandler);
		}

		[TestMethod]
		public void MustRedirectToDisablePdfToolbar()
		{
			var browser = new Mock<IWebBrowser>();
			var headers = new NameValueCollection { { "Content-Type", MediaTypeNames.Application.Pdf } };
			var request = new Mock<IRequest>();
			var response = new Mock<IResponse>();
			var url = "http://www.host.org/some-document";

			request.SetupGet(r => r.ResourceType).Returns(ResourceType.MainFrame);
			request.SetupGet(r => r.Url).Returns(url);
			response.SetupGet(r => r.Headers).Returns(headers);
			settings.AllowPdfReader = true;
			settings.AllowPdfReaderToolbar = false;

			var result = sut.OnResourceResponse(browser.Object, Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, response.Object);

			browser.Verify(b => b.Load(It.Is<string>(s => s.Equals($"{url}#toolbar=0"))), Times.Once);
			Assert.IsTrue(result);

			browser.Reset();
			request.SetupGet(r => r.Url).Returns($"{url}#toolbar=0");

			result = sut.OnResourceResponse(browser.Object, Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, response.Object);

			browser.Verify(b => b.Load(It.IsAny<string>()), Times.Never);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void MustReplaceSebScheme()
		{
			var request = new Mock<IRequest>();
			var url = default(string);

			appConfig.SebUriScheme = "abc";
			appConfig.SebUriSchemeSecure = "abcs";
			request.SetupGet(r => r.Headers).Returns(new NameValueCollection());
			request.SetupGet(r => r.Url).Returns($"{appConfig.SebUriScheme}://www.host.org");
			request.SetupSet(r => r.Url = It.IsAny<string>()).Callback<string>(u => url = u);

			var result = sut.OnBeforeResourceLoad(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, Mock.Of<IRequestCallback>());

			Assert.AreEqual(CefReturnValue.Continue, result);
			Assert.AreEqual("http://www.host.org/", url);

			request.SetupGet(r => r.Url).Returns($"{appConfig.SebUriSchemeSecure}://www.host.org");
			request.SetupSet(r => r.Url = It.IsAny<string>()).Callback<string>(u => url = u);

			result = sut.OnBeforeResourceLoad(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, Mock.Of<IRequestCallback>());

			Assert.AreEqual(CefReturnValue.Continue, result);
			Assert.AreEqual("https://www.host.org/", url);
		}

		private class TestableResourceHandler : ResourceHandler
		{
			internal TestableResourceHandler(
				AppConfig appConfig,
				IRequestFilter filter,
				ILogger logger,
				BrowserSettings settings,
				WindowSettings windowSettings,
				IText text) : base(appConfig, filter, logger, settings, windowSettings, text)
			{
			}

			public new IResourceHandler GetResourceHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
			{
				return base.GetResourceHandler(webBrowser, browser, frame, request);
			}

			public new CefReturnValue OnBeforeResourceLoad(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
			{
				return base.OnBeforeResourceLoad(webBrowser, browser, frame, request, callback);
			}

			public new bool OnResourceResponse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
			{
				return base.OnResourceResponse(webBrowser, browser, frame, request, response);
			}
		}
	}
}
