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
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using SafeExamBrowser.Settings.Browser.Proxy;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;
using ResourceHandler = SafeExamBrowser.Browser.Handlers.ResourceHandler;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class RequestHandlerTests
	{
		private AppConfig appConfig;
		private Mock<IRequestFilter> filter;
		private Mock<ILogger> logger;
		private BrowserSettings settings;
		private WindowSettings windowSettings;
		private ResourceHandler resourceHandler;
		private Mock<IText> text;
		private TestableRequestHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			filter = new Mock<IRequestFilter>();
			logger = new Mock<ILogger>();
			settings = new BrowserSettings();
			windowSettings = new WindowSettings();
			text = new Mock<IText>();
			resourceHandler = new ResourceHandler(appConfig, filter.Object, logger.Object, settings, windowSettings, text.Object);

			sut = new TestableRequestHandler(appConfig, filter.Object, logger.Object, resourceHandler, settings, windowSettings, text.Object);
		}

		[TestMethod]
		public void MustDetectQuitUrl()
		{
			var eventFired = false;
			var quitUrl = "http://www.byebye.com";
			var request = new Mock<IRequest>();

			request.SetupGet(r => r.Url).Returns(quitUrl);
			settings.QuitUrl = quitUrl;
			sut.QuitUrlVisited += (url) => eventFired = true;

			var blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			Assert.IsTrue(blocked);
			Assert.IsTrue(eventFired);

			blocked = false;
			eventFired = false;
			request.SetupGet(r => r.Url).Returns("http://www.bye.com");

			blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			Assert.IsFalse(blocked);
			Assert.IsFalse(eventFired);
		}

		[TestMethod]
		public void MustIgnoreTrailingSlashForQuitUrl()
		{
			var eventFired = false;
			var quitUrl = "http://www.byebye.com";
			var request = new Mock<IRequest>();

			request.SetupGet(r => r.Url).Returns($"{quitUrl}/");
			settings.QuitUrl = quitUrl;
			sut.QuitUrlVisited += (url) => eventFired = true;

			var blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			Assert.IsTrue(blocked);
			Assert.IsTrue(eventFired);

			blocked = false;
			eventFired = false;
			request.SetupGet(r => r.Url).Returns(quitUrl);
			settings.QuitUrl = $"{quitUrl}/";

			blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			Assert.IsTrue(blocked);
			Assert.IsTrue(eventFired);
		}

		[TestMethod]
		public void MustFilterMainRequests()
		{
			var eventFired = false;
			var request = new Mock<IRequest>();
			var url = "www.test.org";

			filter.Setup(f => f.Process(It.Is<Request>(r => r.Url.Equals(url)))).Returns(FilterResult.Block);
			request.SetupGet(r => r.ResourceType).Returns(ResourceType.MainFrame);
			request.SetupGet(r => r.Url).Returns(url);
			settings.Filter.ProcessContentRequests = false;
			settings.Filter.ProcessMainRequests = true;
			sut.RequestBlocked += (u) => eventFired = true;

			var blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			filter.Verify(f => f.Process(It.Is<Request>(r => r.Url.Equals(url))), Times.Once);

			Assert.IsTrue(blocked);
			Assert.IsTrue(eventFired);

			blocked = false;
			eventFired = false;
			request.SetupGet(r => r.ResourceType).Returns(ResourceType.SubFrame);

			blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			filter.Verify(f => f.Process(It.Is<Request>(r => r.Url.Equals(url))), Times.Once);

			Assert.IsFalse(blocked);
			Assert.IsFalse(eventFired);
		}

		[TestMethod]
		public void MustFilterContentRequests()
		{
			var eventFired = false;
			var request = new Mock<IRequest>();
			var url = "www.test.org";

			filter.Setup(f => f.Process(It.Is<Request>(r => r.Url.Equals(url)))).Returns(FilterResult.Block);
			request.SetupGet(r => r.ResourceType).Returns(ResourceType.SubFrame);
			request.SetupGet(r => r.Url).Returns(url);
			settings.Filter.ProcessContentRequests = true;
			settings.Filter.ProcessMainRequests = false;
			sut.RequestBlocked += (u) => eventFired = true;

			var blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			filter.Verify(f => f.Process(It.Is<Request>(r => r.Url.Equals(url))), Times.Once);

			Assert.IsTrue(blocked);
			Assert.IsFalse(eventFired);

			blocked = false;
			eventFired = false;
			request.SetupGet(r => r.ResourceType).Returns(ResourceType.MainFrame);

			blocked = sut.OnBeforeBrowse(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), Mock.Of<IFrame>(), request.Object, false, false);

			filter.Verify(f => f.Process(It.Is<Request>(r => r.Url.Equals(url))), Times.Once);

			Assert.IsFalse(blocked);
			Assert.IsFalse(eventFired);
		}

		[TestMethod]
		public void MustUseProxyCredentials()
		{
			var callback = new Mock<IAuthCallback>();
			var proxy1 = new ProxyConfiguration { Host = "www.test.com", Username = "Sepp", Password = "1234", Port = 10, RequiresAuthentication = true };
			var proxy2 = new ProxyConfiguration { Host = "www.nope.com", Username = "Peter", Password = "4321", Port = 10, RequiresAuthentication = false };

			settings.Proxy.Proxies.Add(proxy1);
			settings.Proxy.Proxies.Add(proxy2);

			var result = sut.GetAuthCredentials(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), default(string), true, "WWW.tEst.Com", 10, default(string), default(string), callback.Object);

			callback.Verify(c => c.Cancel(), Times.Never);
			callback.Verify(c => c.Continue(It.Is<string>(u => u.Equals(proxy1.Username)), It.Is<string>(p => p.Equals(proxy1.Password))), Times.Once);
			callback.Verify(c => c.Continue(It.Is<string>(u => u.Equals(proxy2.Username)), It.Is<string>(p => p.Equals(proxy2.Password))), Times.Never);

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void MustNotUseProxyCredentialsIfNoProxy()
		{
			var callback = new Mock<IAuthCallback>();

			sut.GetAuthCredentials(Mock.Of<IWebBrowser>(), Mock.Of<IBrowser>(), default(string), false, default(string), default(int), default(string), default(string), callback.Object);

			callback.Verify(c => c.Cancel(), Times.Never);
			callback.Verify(c => c.Continue(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		private class TestableRequestHandler : RequestHandler
		{
			internal TestableRequestHandler(
				AppConfig appConfig,
				IRequestFilter filter,
				ILogger logger,
				ResourceHandler resourceHandler,
				BrowserSettings settings,
				WindowSettings windowSettings,
				IText text) : base(appConfig, filter, logger, resourceHandler, settings, windowSettings, text)
			{
			}

			public new bool GetAuthCredentials(IWebBrowser webBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
			{
				return base.GetAuthCredentials(webBrowser, browser, originUrl, isProxy, host, port, realm, scheme, callback);
			}

			public new bool OnBeforeBrowse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
			{
				return base.OnBeforeBrowse(webBrowser, browser, frame, request, userGesture, isRedirect);
			}
		}
	}
}
