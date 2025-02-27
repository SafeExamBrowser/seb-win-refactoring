/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class BrowserResponsibilityTests
	{
		private AppConfig appConfig;
		private Mock<IBrowserApplication> browser;
		private ClientContext context;
		private Mock<ICoordinator> coordinator;
		private Mock<IMessageBox> messageBox;
		private Mock<IRuntimeProxy> runtime;
		private Mock<IServerProxy> server;
		private AppSettings settings;
		private Mock<ISplashScreen> splashScreen;
		private Mock<ITaskbar> taskbar;

		private BrowserResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();
			var responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();

			appConfig = new AppConfig();
			browser = new Mock<IBrowserApplication>();
			context = new ClientContext();
			coordinator = new Mock<ICoordinator>();
			messageBox = new Mock<IMessageBox>();
			runtime = new Mock<IRuntimeProxy>();
			server = new Mock<IServerProxy>();
			settings = new AppSettings();
			splashScreen = new Mock<ISplashScreen>();
			taskbar = new Mock<ITaskbar>();

			context.AppConfig = appConfig;
			context.Browser = browser.Object;
			context.Responsibilities = responsibilities.Object;
			context.Runtime = runtime.Object;
			context.Server = server.Object;
			context.Settings = settings;

			sut = new BrowserResponsibility(
				context,
				coordinator.Object,
				logger.Object,
				messageBox.Object,
				runtime.Object,
				splashScreen.Object,
				taskbar.Object);

			sut.Assume(ClientTask.RegisterEvents);
		}

		[TestMethod]
		public void MustAutoStartBrowser()
		{
			settings.Browser.EnableBrowser = true;
			browser.SetupGet(b => b.AutoStart).Returns(true);

			sut.Assume(ClientTask.AutoStartApplications);

			browser.Verify(b => b.Start(), Times.Once);
			browser.Reset();
			browser.SetupGet(b => b.AutoStart).Returns(false);

			sut.Assume(ClientTask.AutoStartApplications);

			browser.Verify(b => b.Start(), Times.Never);
		}

		[TestMethod]
		public void MustNotAutoStartBrowserIfNotEnabled()
		{
			settings.Browser.EnableBrowser = false;
			browser.SetupGet(b => b.AutoStart).Returns(true);

			sut.Assume(ClientTask.AutoStartApplications);

			browser.Verify(b => b.Start(), Times.Never);
		}

		[TestMethod]
		public void Browser_MustHandleUserIdentifierDetection()
		{
			var counter = 0;
			var identifier = "abc123";

			settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.SendUserIdentifier(It.IsAny<string>())).Returns(() => new ServerResponse(++counter == 3));

			browser.Raise(b => b.UserIdentifierDetected += null, identifier);

			server.Verify(s => s.SendUserIdentifier(It.Is<string>(id => id == identifier)), Times.Exactly(3));
		}

		[TestMethod]
		public void Browser_MustTerminateIfRequested()
		{
			runtime.Setup(p => p.RequestShutdown()).Returns(new CommunicationResult(true));

			browser.Raise(b => b.TerminationRequested += null);

			runtime.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void Reconfiguration_MustAllowIfNoQuitPasswordSet()
		{
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(true);
			runtime.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);
			args.Callback(true, string.Empty);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustNotAllowWithQuitPasswordAndNoUrl()
		{
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			settings.Security.AllowReconfiguration = true;
			settings.Security.QuitPasswordHash = "abc123";
			runtime.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Never);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustNotAllowConcurrentExecution()
		{
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(false);
			runtime.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);
			args.Callback?.Invoke(true, string.Empty);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustAllowIfUrlMatches()
		{
			var args = new DownloadEventArgs { Url = "sebs://www.somehost.org/some/path/some_configuration.seb?query=123" };

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(true);
			settings.Security.AllowReconfiguration = true;
			settings.Security.QuitPasswordHash = "abc123";
			settings.Security.ReconfigurationUrl = "sebs://www.somehost.org/some/path/*.seb?query=123";
			runtime.Setup(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>())).Returns(new CommunicationResult(true));

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);
			args.Callback(true, string.Empty);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustDenyIfNotAllowed()
		{
			var args = new DownloadEventArgs();

			settings.Security.AllowReconfiguration = false;
			settings.Security.QuitPasswordHash = "abc123";

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Never);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustDenyIfUrlDoesNotMatch()
		{
			var args = new DownloadEventArgs { Url = "sebs://www.somehost.org/some/path/some_configuration.seb?query=123" };

			settings.Security.AllowReconfiguration = false;
			settings.Security.QuitPasswordHash = "abc123";
			settings.Security.ReconfigurationUrl = "sebs://www.somehost.org/some/path/other_configuration.seb?query=123";

			browser.Raise(b => b.ConfigurationDownloadRequested += null, "filepath.seb", args);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Never);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			Assert.IsFalse(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(true);
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtime.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(true));
			settings.Security.AllowReconfiguration = true;

			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadUrl, downloadPath);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Never);
			runtime.Verify(r => r.RequestReconfiguration(It.Is<string>(p => p == downloadPath), It.Is<string>(u => u == downloadUrl)), Times.Once);

			Assert.AreEqual(downloadPath, args.DownloadPath);
			Assert.IsTrue(args.AllowDownload);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedDownload()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(true);
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtime.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(true));
			settings.Security.AllowReconfiguration = true;

			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(false, downloadPath);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Once);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void Reconfiguration_MustCorrectlyHandleFailedRequest()
		{
			var downloadPath = @"C:\Folder\Does\Not\Exist\filepath.seb";
			var downloadUrl = @"https://www.host.abc/someresource.seb";
			var filename = "filepath.seb";
			var args = new DownloadEventArgs();

			appConfig.TemporaryDirectory = @"C:\Folder\Does\Not\Exist";
			coordinator.Setup(c => c.RequestReconfigurationLock()).Returns(true);
			messageBox.Setup(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>())).Returns(MessageBoxResult.Yes);
			runtime.Setup(r => r.RequestReconfiguration(
				It.Is<string>(p => p == downloadPath),
				It.Is<string>(u => u == downloadUrl))).Returns(new CommunicationResult(false));
			settings.Security.AllowReconfiguration = true;

			browser.Raise(b => b.ConfigurationDownloadRequested += null, filename, args);
			args.Callback(true, downloadUrl, downloadPath);

			coordinator.Verify(c => c.RequestReconfigurationLock(), Times.Once);
			coordinator.Verify(c => c.ReleaseReconfigurationLock(), Times.Once);
			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			runtime.Verify(r => r.RequestReconfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		[TestMethod]
		public void MustNotFailIfDependencyIsNull()
		{
			context.Browser = null;
			sut.Assume(ClientTask.DeregisterEvents);
		}
	}
}
