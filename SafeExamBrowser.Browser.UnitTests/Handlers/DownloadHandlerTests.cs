/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Threading;
using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class DownloadHandlerTests
	{
		private AppConfig appConfig;
		private Mock<ILogger> logger;
		private BrowserSettings settings;
		private WindowSettings windowSettings;
		private DownloadHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			logger = new Mock<ILogger>();
			settings = new BrowserSettings();
			windowSettings = new WindowSettings();

			sut = new DownloadHandler(appConfig, logger.Object, settings, windowSettings);
		}

		[TestMethod]
		public void MustCorrectlyHandleConfigurationByFileExtension()
		{
			var item = new DownloadItem
			{
				SuggestedFileName = "File.seb",
				Url = "https://somehost.org/some-path"
			};

			RequestConfigurationDownload(item);
		}

		[TestMethod]
		public void MustCorrectlyHandleConfigurationByUrlExtension()
		{
			var item = new DownloadItem
			{
				SuggestedFileName = "Abc.xyz",
				Url = "https://somehost.org/some-path-to/file.seb"
			};

			RequestConfigurationDownload(item);
		}

		[TestMethod]
		public void MustCorrectlyHandleConfigurationByMimeType()
		{
			appConfig.ConfigurationFileMimeType = "some/mime-type";

			var item = new DownloadItem
			{
				MimeType = appConfig.ConfigurationFileMimeType,
				SuggestedFileName = "Abc.xyz",
				Url = "https://somehost.org/some-path"
			};

			RequestConfigurationDownload(item);
		}

		[TestMethod]
		public void MustCorrectlyHandleDeniedConfigurationFileDownload()
		{
			var args = default(DownloadEventArgs);
			var callback = new Mock<IBeforeDownloadCallback>();
			var failed = false;
			var fileName = default(string);
			var item = new DownloadItem
			{
				SuggestedFileName = "File.seb",
				Url = "https://somehost.org/some-path"
			};
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			settings.AllowDownloads = false;
			settings.AllowConfigurationDownloads = true;
			sut.ConfigurationDownloadRequested += (f, a) =>
			{
				args = a;
				args.AllowDownload = false;
				fileName = f;
				threadId = Thread.CurrentThread.ManagedThreadId;
				sync.Set();
			};
			sut.DownloadUpdated += (state) => failed = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);
			sync.WaitOne();

			callback.VerifyNoOtherCalls();

			Assert.IsFalse(failed);
			Assert.IsFalse(args.AllowDownload);
			Assert.AreEqual(item.SuggestedFileName, fileName);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		public void MustCorrectlyHandleFileDownload()
		{
			var callback = new Mock<IBeforeDownloadCallback>();
			var downloadPath = default(string);
			var failed = false;
			var item = new DownloadItem
			{
				MimeType = "application/something",
				SuggestedFileName = "File.txt",
				Url = "https://somehost.org/somefile.abc"
			};
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			callback.Setup(c => c.Continue(It.IsAny<string>(), It.IsAny<bool>())).Callback<string, bool>((f, s) =>
			{
				downloadPath = f;
				threadId = Thread.CurrentThread.ManagedThreadId;
				sync.Set();
			});
			settings.AllowDownloads = true;
			settings.AllowConfigurationDownloads = false;
			sut.ConfigurationDownloadRequested += (f, a) => failed = true;
			sut.DownloadUpdated += (state) => failed = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);
			sync.WaitOne();

			callback.Verify(c => c.Continue(It.Is<string>(p => p.Equals(downloadPath)), false), Times.Once);

			Assert.IsFalse(failed);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		public void MustCorrectlyHandleFileDownloadWithCustomDirectory()
		{
			var callback = new Mock<IBeforeDownloadCallback>();
			var failed = false;
			var item = new DownloadItem
			{
				MimeType = "application/something",
				SuggestedFileName = "File.txt",
				Url = "https://somehost.org/somefile.abc"
			};
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			callback.Setup(c => c.Continue(It.IsAny<string>(), It.IsAny<bool>())).Callback(() =>
			{
				threadId = Thread.CurrentThread.ManagedThreadId;
				sync.Set();
			});
			settings.AllowDownloads = true;
			settings.AllowConfigurationDownloads = false;
			settings.AllowCustomDownAndUploadLocation = true;
			settings.DownAndUploadDirectory = @"%APPDATA%\Downloads";
			sut.ConfigurationDownloadRequested += (f, a) => failed = true;
			sut.DownloadUpdated += (state) => failed = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);
			sync.WaitOne();

			var downloadPath = Path.Combine(Environment.ExpandEnvironmentVariables(settings.DownAndUploadDirectory), item.SuggestedFileName);

			callback.Verify(c => c.Continue(It.Is<string>(p => p.Equals(downloadPath)), true), Times.Once);

			Assert.IsFalse(failed);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		public void MustDoNothingIfDownloadsNotAllowed()
		{
			var callback = new Mock<IBeforeDownloadCallback>();
			var fail = false;
			var item = new DownloadItem
			{
				SuggestedFileName = "File.txt",
				Url = "https://somehost.org/somefile.abc"
			};

			settings.AllowDownloads = false;
			settings.AllowConfigurationDownloads = false;
			sut.ConfigurationDownloadRequested += (file, args) => fail = true;
			sut.DownloadUpdated += (state) => fail = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);

			callback.VerifyNoOtherCalls();
			Assert.IsFalse(fail);
		}

		[TestMethod]
		public void MustUpdateDownloadProgress()
		{
			var callback = new Mock<IBeforeDownloadCallback>();
			var failed = false;
			var item = new DownloadItem
			{
				MimeType = "application/something",
				SuggestedFileName = "File.txt",
				Url = "https://somehost.org/somefile.abc"
			};
			var state = default(DownloadItemState);
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			callback.Setup(c => c.Continue(It.IsAny<string>(), It.IsAny<bool>())).Callback(() => sync.Set());
			settings.AllowDownloads = true;
			settings.AllowConfigurationDownloads = false;
			sut.ConfigurationDownloadRequested += (f, a) => failed = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);
			sync.WaitOne();

			Assert.IsFalse(failed);

			sut.DownloadUpdated += (s) =>
			{
				state = s;
				threadId = Thread.CurrentThread.ManagedThreadId;
				sync.Set();
			};

			item.PercentComplete = 10;
			sut.OnDownloadUpdated(default(IWebBrowser), default(IBrowser), item, default(IDownloadItemCallback));
			sync.WaitOne();

			Assert.IsFalse(state.IsCancelled);
			Assert.IsFalse(state.IsComplete);
			Assert.AreEqual(0.1, state.Completion);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);

			item.PercentComplete = 20;
			sut.OnDownloadUpdated(default(IWebBrowser), default(IBrowser), item, default(IDownloadItemCallback));
			sync.WaitOne();

			Assert.IsFalse(state.IsCancelled);
			Assert.IsFalse(state.IsComplete);
			Assert.AreEqual(0.2, state.Completion);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);

			item.PercentComplete = 50;
			item.IsCancelled = true;
			sut.OnDownloadUpdated(default(IWebBrowser), default(IBrowser), item, default(IDownloadItemCallback));
			sync.WaitOne();

			Assert.IsFalse(failed);
			Assert.IsTrue(state.IsCancelled);
			Assert.IsFalse(state.IsComplete);
			Assert.AreEqual(0.5, state.Completion);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		private void RequestConfigurationDownload(DownloadItem item)
		{
			var args = default(DownloadEventArgs);
			var callback = new Mock<IBeforeDownloadCallback>();
			var failed = false;
			var fileName = default(string);
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			callback.Setup(c => c.Continue(It.IsAny<string>(), It.IsAny<bool>())).Callback(() => sync.Set());
			settings.AllowDownloads = false;
			settings.AllowConfigurationDownloads = true;
			sut.ConfigurationDownloadRequested += (f, a) =>
			{
				args = a;
				args.AllowDownload = true;
				args.DownloadPath = @"C:\Downloads\File.seb";
				fileName = f;
				threadId = Thread.CurrentThread.ManagedThreadId;
			};
			sut.DownloadUpdated += (state) => failed = true;

			sut.OnBeforeDownload(default(IWebBrowser), default(IBrowser), item, callback.Object);
			sync.WaitOne();

			callback.Verify(c => c.Continue(It.Is<string>(p => p.Equals(args.DownloadPath)), false), Times.Once);

			Assert.IsFalse(failed);
			Assert.IsTrue(args.AllowDownload);
			Assert.AreEqual(item.SuggestedFileName, fileName);
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}
	}
}
