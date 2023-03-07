/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Net.Mime;
using System.Threading;
using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
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
		private Mock<IKeyGenerator> keyGenerator;
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
			keyGenerator = new Mock<IKeyGenerator>();
			logger = new Mock<ILogger>();
			settings = new BrowserSettings();
			windowSettings = new WindowSettings();
			text = new Mock<IText>();

			sut = new TestableResourceHandler(appConfig, filter.Object, keyGenerator.Object, logger.Object, SessionMode.Server, settings, windowSettings, text.Object);
		}

		[TestMethod]
		public void MustAppendCustomHeadersForSameDomain()
		{
			var browser = new Mock<IWebBrowser>();
			var headers = default(NameValueCollection);
			var request = new Mock<IRequest>();

			browser.SetupGet(b => b.Address).Returns("http://www.host.org");
			keyGenerator.Setup(g => g.CalculateBrowserExamKeyHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>())).Returns(new Random().Next().ToString());
			keyGenerator.Setup(g => g.CalculateConfigurationKeyHash(It.IsAny<string>(), It.IsAny<string>())).Returns(new Random().Next().ToString());
			request.SetupGet(r => r.Headers).Returns(new NameValueCollection());
			request.SetupGet(r => r.Url).Returns("http://www.host.org");
			request.SetupSet(r => r.Headers = It.IsAny<NameValueCollection>()).Callback<NameValueCollection>((h) => headers = h);
			settings.SendConfigurationKey = true;
			settings.SendBrowserExamKey = true;

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
			settings.SendBrowserExamKey = true;

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
			var url = "http://www.test.org";

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
		public void MustLetOperatingSystemHandleUnknownProtocols()
		{
			Assert.IsTrue(sut.OnProtocolExecution(default(IWebBrowser), default(IBrowser), default(IFrame), default(IRequest)));
		}

		[TestMethod]
		public void MustRedirectToDisablePdfToolbar()
		{
			var frame = new Mock<IFrame>();
			var headers = new NameValueCollection { { "Content-Type", MediaTypeNames.Application.Pdf } };
			var request = new Mock<IRequest>();
			var response = new Mock<IResponse>();
			var url = "http://www.host.org/some-document";

			request.SetupGet(r => r.ResourceType).Returns(ResourceType.MainFrame);
			request.SetupGet(r => r.Url).Returns(url);
			response.SetupGet(r => r.Headers).Returns(headers);
			settings.AllowPdfReader = true;
			settings.AllowPdfReaderToolbar = false;

			var result = sut.OnResourceResponse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), frame.Object, request.Object, response.Object);

			frame.Verify(b => b.LoadUrl(It.Is<string>(s => s.Equals($"{url}#toolbar=0"))), Times.Once);
			Assert.IsTrue(result);

			frame.Reset();
			request.SetupGet(r => r.Url).Returns($"{url}#toolbar=0");

			result = sut.OnResourceResponse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), frame.Object, request.Object, response.Object);

			frame.Verify(b => b.LoadUrl(It.IsAny<string>()), Times.Never);
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

		[TestMethod]
		public void MustSearchGenericLmsSessionIdentifier()
		{
			var @event = new AutoResetEvent(false);
			var headers = new NameValueCollection();
			var newUrl = default(string);
			var request = new Mock<IRequest>();
			var response = new Mock<IResponse>();
			var sessionId = default(string);

			headers.Add("X-LMS-USER-ID", "some-session-id-123");
			request.SetupGet(r => r.Url).Returns("https://www.somelms.org");
			response.SetupGet(r => r.Headers).Returns(headers);
			sut.SessionIdentifierDetected += (id) =>
			{
				sessionId = id;
				@event.Set();
			};

			sut.OnResourceRedirect(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IRequest>(), response.Object, ref newUrl);
			@event.WaitOne();
			Assert.AreEqual("some-session-id-123", sessionId);

			headers.Clear();
			headers.Add("X-LMS-USER-ID", "other-session-id-123");
			sessionId = default(string);

			sut.OnResourceResponse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, response.Object);
			@event.WaitOne();
			Assert.AreEqual("other-session-id-123", sessionId);
		}

		[TestMethod]
		public void MustSearchEdxSessionIdentifier()
		{
			var @event = new AutoResetEvent(false);
			var headers = new NameValueCollection();
			var newUrl = default(string);
			var request = new Mock<IRequest>();
			var response = new Mock<IResponse>();
			var sessionId = default(string);

			headers.Add("Set-Cookie", "edx-user-info=\"{\\\"username\\\": \\\"edx-123\\\"}\"; expires");
			request.SetupGet(r => r.Url).Returns("https://www.somelms.org");
			response.SetupGet(r => r.Headers).Returns(headers);
			sut.SessionIdentifierDetected += (id) =>
			{
				sessionId = id;
				@event.Set();
			};

			sut.OnResourceRedirect(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IRequest>(), response.Object, ref newUrl);
			@event.WaitOne();
			Assert.AreEqual("edx-123", sessionId);

			headers.Clear();
			headers.Add("Set-Cookie", "edx-user-info=\"{\\\"username\\\": \\\"edx-345\\\"}\"; expires");
			sessionId = default(string);

			sut.OnResourceResponse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, response.Object);
			@event.WaitOne();
			Assert.AreEqual("edx-345", sessionId);
		}

		[TestMethod]
		public void MustSearchMoodleSessionIdentifier()
		{
			var @event = new AutoResetEvent(false);
			var headers = new NameValueCollection();
			var newUrl = default(string);
			var request = new Mock<IRequest>();
			var response = new Mock<IResponse>();
			var sessionId = default(string);

			headers.Add("Location", "https://www.some-moodle-instance.org/moodle/login/index.php?testsession=123");
			request.SetupGet(r => r.Url).Returns("https://www.some-moodle-instance.org");
			response.SetupGet(r => r.Headers).Returns(headers);
			sut.SessionIdentifierDetected += (id) =>
			{
				sessionId = id;
				@event.Set();
			};

			sut.OnResourceRedirect(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), Mock.Of<IRequest>(), response.Object, ref newUrl);
			@event.WaitOne();
			Assert.AreEqual("123", sessionId);

			headers.Clear();
			headers.Add("Location", "https://www.some-moodle-instance.org/moodle/login/index.php?testsession=456");
			sessionId = default(string);

			sut.OnResourceResponse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, response.Object);
			@event.WaitOne();
			Assert.AreEqual("456", sessionId);
		}

		private class TestableResourceHandler : ResourceHandler
		{
			internal TestableResourceHandler(
				AppConfig appConfig,
				IRequestFilter filter,
				IKeyGenerator keyGenerator,
				ILogger logger,
				SessionMode sessionMode,
				BrowserSettings settings,
				WindowSettings windowSettings,
				IText text) : base(appConfig, filter, keyGenerator, logger, sessionMode, settings, windowSettings, text)
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

			public new bool OnProtocolExecution(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
			{
				return base.OnProtocolExecution(webBrowser, browser, frame, request);
			}

			public new void OnResourceRedirect(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
			{
				base.OnResourceRedirect(webBrowser, browser, frame, request, response, ref newUrl);
			}

			public new bool OnResourceResponse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
			{
				return base.OnResourceResponse(webBrowser, browser, frame, request, response);
			}
		}
	}
}
