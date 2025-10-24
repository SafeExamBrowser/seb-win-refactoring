/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Server;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class ServerOperationTests
	{
		private RuntimeContext context;
		private Mock<IFileSystem> fileSystem;
		private Mock<IConfigurationRepository> repository;
		private Mock<IServerProxy> server;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ServerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			fileSystem = new Mock<IFileSystem>();
			repository = new Mock<IConfigurationRepository>();
			server = new Mock<IServerProxy>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			context.Current = new SessionConfiguration();
			context.Current.AppConfig = new AppConfig();
			context.Current.Settings = new AppSettings();
			context.Next = new SessionConfiguration();
			context.Next.AppConfig = new AppConfig();
			context.Next.Settings = new AppSettings();

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), context),
				Mock.Of<ILogger>(),
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				context,
				Mock.Of<IText>());

			sut = new ServerOperation(dependencies, fileSystem.Object, repository.Object, server.Object, uiFactory.Object);
		}

		[TestMethod]
		public void Perform_MustCorrectlyInitializeServerSession()
		{
			var connect = 0;
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var counter = 0;
			var delete = 0;
			var dialog = new Mock<IExamSelectionDialog>();
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSelection = 0;
			var examSettings = new AppSettings();
			var getConfiguration = 0;
			var getConnection = 0;
			var getExams = 0;
			var initialize = 0;
			var initialSettings = context.Next.Settings;
			var serverSettings = context.Next.Settings.Server;

			dialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true })
				.Callback(() => examSelection = ++counter);
			repository
				.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => delete = ++counter);
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true)).Callback(() => connect = ++counter);
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>())).Callback(() => initialize = ++counter);
			server.Setup(s => s.GetConnectionInfo()).Returns(connection).Callback(() => getConnection = ++counter);
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			server
				.Setup(s => s.GetAvailableExams(It.IsAny<string>()))
				.Returns(new ServerResponse<IEnumerable<Exam>>(true, default))
				.Callback(() => getExams = ++counter);
			server
				.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>()))
				.Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")))
				.Callback(() => getConfiguration = ++counter);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Perform();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);
			server.Verify(s => s.SendSelectedExam(It.Is<Exam>(e => e == exam)), Times.Once);

			Assert.AreEqual(1, initialize);
			Assert.AreEqual(2, connect);
			Assert.AreEqual(3, getExams);
			Assert.AreEqual(4, examSelection);
			Assert.AreEqual(5, getConfiguration);
			Assert.AreEqual(6, getConnection);
			Assert.AreEqual(7, delete);
			Assert.AreEqual(connection.Api, context.Next.AppConfig.ServerApi);
			Assert.AreEqual(connection.ConnectionToken, context.Next.AppConfig.ServerConnectionToken);
			Assert.AreEqual(connection.Oauth2Token, context.Next.AppConfig.ServerOauth2Token);
			Assert.AreEqual(exam.Id, context.Next.AppConfig.ServerExamId);
			Assert.AreEqual(exam.Url, context.Next.Settings.Browser.StartUrl);
			Assert.AreSame(examSettings, context.Next.Settings);
			Assert.AreSame(serverSettings, context.Next.Settings.Server);
			Assert.AreNotSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(SessionMode.Server, context.Next.Settings.SessionMode);
		}

		[TestMethod]
		public void Perform_MustFailIfSettingsCouldNotBeLoaded()
		{
			var dialog = new Mock<IExamSelectionDialog>();
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var initialSettings = context.Next.Settings;

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.UnexpectedError);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Perform();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);

			Assert.AreNotEqual(connection.Api, context.Next.AppConfig.ServerApi);
			Assert.AreNotEqual(connection.ConnectionToken, context.Next.AppConfig.ServerConnectionToken);
			Assert.AreNotEqual(connection.Oauth2Token, context.Next.AppConfig.ServerOauth2Token);
			Assert.AreNotEqual(exam.Id, context.Next.AppConfig.ServerExamId);
			Assert.AreNotEqual(exam.Url, context.Next.Settings.Browser.StartUrl);
			Assert.AreNotSame(examSettings, context.Next.Settings);
			Assert.AreSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(OperationResult.Failed, result);
			Assert.AreEqual(SessionMode.Server, context.Next.Settings.SessionMode);
		}

		[TestMethod]
		public void Perform_MustCorrectlyAbort()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examDialog = new Mock<IExamSelectionDialog>();
			var examSettings = new AppSettings();
			var messageShown = false;
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(false, default));
			serverDialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ServerFailureDialogResult { Abort = true })
				.Callback(() => messageShown = true);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Perform();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Never);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Never);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustCorrectlyFallback()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examDialog = new Mock<IExamSelectionDialog>();
			var examSettings = new AppSettings();
			var messageShown = false;
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(false, default));
			serverDialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ServerFailureDialogResult { Fallback = true })
				.Callback(() => messageShown = true);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Perform();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Never);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Never);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(SessionMode.Normal, context.Next.Settings.SessionMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAutomaticallySelectExam()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var serverSettings = context.Next.Settings.Server;
			var examDialog = new Mock<IExamSelectionDialog>();
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(Assert.Fail);
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			context.Next.Settings.Server.ExamId = "some id";
			fileSystem.Setup(f => f.Delete(It.IsAny<string>()));
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, new[] { exam }));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			serverDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(Assert.Fail);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Perform();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);
			server.Verify(s => s.SendSelectedExam(It.Is<Exam>(e => e == exam)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustDoNothingIfNormalSession()
		{
			var initialSettings = context.Next.Settings;

			context.Next.Settings.SessionMode = SessionMode.Normal;

			var result = sut.Perform();

			repository.VerifyNoOtherCalls();
			fileSystem.VerifyNoOtherCalls();
			server.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();

			Assert.AreSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(SessionMode.Normal, context.Next.Settings.SessionMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSetCustomBrowserExamKey()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var browserExamKey = "BEK-TEST-1234";
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var serverSettings = context.Next.Settings.Server;
			var examDialog = new Mock<IExamSelectionDialog>();
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(Assert.Fail);
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			context.Next.Settings.Server.ExamId = "some id";
			fileSystem.Setup(f => f.Delete(It.IsAny<string>()));
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, new[] { exam }));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, browserExamKey));
			serverDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(Assert.Fail);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Perform();

			Assert.AreEqual(browserExamKey, context.Next.Settings.Browser.CustomBrowserExamKey);
			Assert.AreEqual(OperationResult.Success, result);

		}

		[TestMethod]
		public void Repeat_MustCorrectlyInitializeServerSession()
		{
			var connect = 0;
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var counter = 0;
			var delete = 0;
			var dialog = new Mock<IExamSelectionDialog>();
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSelection = 0;
			var examSettings = new AppSettings();
			var getConfiguration = 0;
			var getConnection = 0;
			var getExams = 0;
			var initialize = 0;
			var initialSettings = context.Next.Settings;
			var serverSettings = context.Next.Settings.Server;

			dialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true })
				.Callback(() => examSelection = ++counter);
			repository
				.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => delete = ++counter);
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true)).Callback(() => connect = ++counter);
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>())).Callback(() => initialize = ++counter);
			server.Setup(s => s.GetConnectionInfo()).Returns(connection).Callback(() => getConnection = ++counter);
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			server
				.Setup(s => s.GetAvailableExams(It.IsAny<string>()))
				.Returns(new ServerResponse<IEnumerable<Exam>>(true, default))
				.Callback(() => getExams = ++counter);
			server
				.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>()))
				.Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")))
				.Callback(() => getConfiguration = ++counter);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);
			server.Verify(s => s.SendSelectedExam(It.Is<Exam>(e => e == exam)), Times.Once);

			Assert.AreEqual(1, initialize);
			Assert.AreEqual(2, connect);
			Assert.AreEqual(3, getExams);
			Assert.AreEqual(4, examSelection);
			Assert.AreEqual(5, getConfiguration);
			Assert.AreEqual(6, getConnection);
			Assert.AreEqual(7, delete);
			Assert.AreEqual(connection.Api, context.Next.AppConfig.ServerApi);
			Assert.AreEqual(connection.ConnectionToken, context.Next.AppConfig.ServerConnectionToken);
			Assert.AreEqual(connection.Oauth2Token, context.Next.AppConfig.ServerOauth2Token);
			Assert.AreEqual(exam.Id, context.Next.AppConfig.ServerExamId);
			Assert.AreEqual(exam.Url, context.Next.Settings.Browser.StartUrl);
			Assert.AreSame(examSettings, context.Next.Settings);
			Assert.AreSame(serverSettings, context.Next.Settings.Server);
			Assert.AreNotSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(SessionMode.Server, context.Next.Settings.SessionMode);
		}

		[TestMethod]
		public void Repeat_MustFailIfSettingsCouldNotBeLoaded()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var initialSettings = context.Next.Settings;
			var dialog = new Mock<IExamSelectionDialog>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.UnexpectedError);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);

			Assert.AreNotEqual(connection.Api, context.Next.AppConfig.ServerApi);
			Assert.AreNotEqual(connection.ConnectionToken, context.Next.AppConfig.ServerConnectionToken);
			Assert.AreNotEqual(connection.Oauth2Token, context.Next.AppConfig.ServerOauth2Token);
			Assert.AreNotEqual(exam.Id, context.Next.AppConfig.ServerExamId);
			Assert.AreNotEqual(exam.Url, context.Next.Settings.Browser.StartUrl);
			Assert.AreNotSame(examSettings, context.Next.Settings);
			Assert.AreSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(OperationResult.Failed, result);
			Assert.AreEqual(SessionMode.Server, context.Next.Settings.SessionMode);
		}

		[TestMethod]
		public void Repeat_MustFailIfFinalizationOfCurrentSessionFails()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var dialog = new Mock<IExamSelectionDialog>();
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var initialSettings = context.Next.Settings;
			var serverSettings = context.Next.Settings.Server;

			dialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository
				.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			context.Current.Settings.SessionMode = SessionMode.Server;
			context.Next.Settings.SessionMode = SessionMode.Server;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>()));
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Disconnect()).Returns(new ServerResponse(false));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			server
				.Setup(s => s.GetAvailableExams(It.IsAny<string>()))
				.Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server
				.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>()))
				.Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Repeat();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustCorrectlyAbort()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var messageShown = false;
			var examDialog = new Mock<IExamSelectionDialog>();
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(false, default));
			serverDialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ServerFailureDialogResult { Abort = true })
				.Callback(() => messageShown = true);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Never);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Never);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustCorrectlyFallback()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var messageShown = false;
			var examDialog = new Mock<IExamSelectionDialog>();
			var serverDialog = new Mock<IServerFailureDialog>();

			examDialog.Setup(d => d.Show(It.IsAny<IWindow>())).Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true });
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, default));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(false, default));
			serverDialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ServerFailureDialogResult { Fallback = true })
				.Callback(() => messageShown = true);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(examDialog.Object);
			uiFactory.Setup(f => f.CreateServerFailureDialog(It.IsAny<string>(), It.IsAny<bool>())).Returns(serverDialog.Object);

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Never);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Never);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(SessionMode.Normal, context.Next.Settings.SessionMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustCorrectlyReconfigureServerSession()
		{
			var connect = 0;
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var counter = 0;
			var delete = 0;
			var dialog = new Mock<IExamSelectionDialog>();
			var disconnect = 0;
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSelection = 0;
			var examSettings = new AppSettings();
			var getConfiguration = 0;
			var getConnection = 0;
			var getExams = 0;
			var initialize = 0;
			var initialSettings = context.Next.Settings;
			var serverSettings = context.Next.Settings.Server;

			dialog
				.Setup(d => d.Show(It.IsAny<IWindow>()))
				.Returns(new ExamSelectionDialogResult { SelectedExam = exam, Success = true })
				.Callback(() => examSelection = ++counter);
			repository
				.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>()))
				.Returns(LoadStatus.Success);
			context.Current.Settings.SessionMode = SessionMode.Server;
			context.Next.Settings.SessionMode = SessionMode.Server;
			fileSystem.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => delete = ++counter);
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true)).Callback(() => connect = ++counter);
			server.Setup(s => s.Disconnect()).Returns(new ServerResponse(true)).Callback(() => disconnect = ++counter);
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>())).Callback(() => initialize = ++counter);
			server.Setup(s => s.GetConnectionInfo()).Returns(connection).Callback(() => getConnection = ++counter);
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			server
				.Setup(s => s.GetAvailableExams(It.IsAny<string>()))
				.Returns(new ServerResponse<IEnumerable<Exam>>(true, default))
				.Callback(() => getExams = ++counter);
			server
				.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>()))
				.Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")))
				.Callback(() => getConfiguration = ++counter);
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Repeat();

			Assert.AreEqual(OperationResult.Success, result);

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Disconnect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);
			server.Verify(s => s.SendSelectedExam(It.Is<Exam>(e => e == exam)), Times.Once);

			Assert.AreEqual(1, disconnect);
			Assert.AreEqual(2, initialize);
			Assert.AreEqual(3, connect);
			Assert.AreEqual(4, getExams);
			Assert.AreEqual(5, examSelection);
			Assert.AreEqual(6, getConfiguration);
			Assert.AreEqual(7, getConnection);
			Assert.AreEqual(8, delete);
			Assert.AreEqual(connection.Api, context.Next.AppConfig.ServerApi);
			Assert.AreEqual(connection.ConnectionToken, context.Next.AppConfig.ServerConnectionToken);
			Assert.AreEqual(connection.Oauth2Token, context.Next.AppConfig.ServerOauth2Token);
			Assert.AreEqual(exam.Id, context.Next.AppConfig.ServerExamId);
			Assert.AreEqual(exam.Url, context.Next.Settings.Browser.StartUrl);
			Assert.AreSame(examSettings, context.Next.Settings);
			Assert.AreSame(serverSettings, context.Next.Settings.Server);
			Assert.AreNotSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(OperationResult.Success, result);
			Assert.AreEqual(SessionMode.Server, context.Next.Settings.SessionMode);
		}

		[TestMethod]
		public void Repeat_MustAutomaticallySelectExam()
		{
			var connection = new ConnectionInfo { Api = "some API", ConnectionToken = "some token", Oauth2Token = "some OAuth2 token" };
			var exam = new Exam { Id = "some id", LmsName = "some LMS", Name = "some name", Url = "some URL" };
			var examSettings = new AppSettings();
			var serverSettings = context.Next.Settings.Server;
			var dialog = new Mock<IExamSelectionDialog>();

			dialog.Setup(d => d.Show(It.IsAny<IWindow>())).Callback(Assert.Fail);
			repository.Setup(c => c.TryLoadSettings(It.IsAny<Uri>(), out examSettings, It.IsAny<PasswordParameters>())).Returns(LoadStatus.Success);
			context.Next.Settings.SessionMode = SessionMode.Server;
			context.Next.Settings.Server.ExamId = "some id";
			fileSystem.Setup(f => f.Delete(It.IsAny<string>()));
			server.Setup(s => s.Connect()).Returns(new ServerResponse(true));
			server.Setup(s => s.Initialize(It.IsAny<ServerSettings>()));
			server.Setup(s => s.GetConnectionInfo()).Returns(connection);
			server.Setup(s => s.GetAvailableExams(It.IsAny<string>())).Returns(new ServerResponse<IEnumerable<Exam>>(true, new[] { exam }));
			server.Setup(s => s.GetConfigurationFor(It.IsAny<Exam>())).Returns(new ServerResponse<Uri>(true, new Uri("file:///configuration.seb")));
			server.Setup(s => s.SendSelectedExam(It.IsAny<Exam>())).Returns(new ServerResponse<string>(true, default));
			uiFactory.Setup(f => f.CreateExamSelectionDialog(It.IsAny<IEnumerable<Exam>>())).Returns(dialog.Object);

			var result = sut.Repeat();

			fileSystem.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.Connect(), Times.Once);
			server.Verify(s => s.Initialize(It.IsAny<ServerSettings>()), Times.Once);
			server.Verify(s => s.GetAvailableExams(It.IsAny<string>()), Times.Once);
			server.Verify(s => s.GetConfigurationFor(It.Is<Exam>(e => e == exam)), Times.Once);
			server.Verify(s => s.GetConnectionInfo(), Times.Once);
			server.Verify(s => s.SendSelectedExam(It.Is<Exam>(e => e == exam)), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustDoNothingIfNormalSession()
		{
			var initialSettings = context.Next.Settings;

			context.Current.Settings.SessionMode = SessionMode.Normal;
			context.Next.Settings.SessionMode = SessionMode.Normal;

			var result = sut.Repeat();

			repository.VerifyNoOtherCalls();
			fileSystem.VerifyNoOtherCalls();
			server.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();

			Assert.AreSame(initialSettings, context.Next.Settings);
			Assert.AreEqual(SessionMode.Normal, context.Next.Settings.SessionMode);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDisconnectFromServerWhenSessionRunning()
		{
			context.Current.Settings.SessionMode = SessionMode.Server;
			context.Next = default;
			server.Setup(s => s.Disconnect()).Returns(new ServerResponse(true));

			var result = sut.Revert();

			fileSystem.VerifyNoOtherCalls();
			server.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDisconnectFromServerWhenSessionStartFailed()
		{
			context.Current = default;
			context.Next.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Disconnect()).Returns(new ServerResponse(true));

			var result = sut.Revert();

			fileSystem.VerifyNoOtherCalls();
			server.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustFailWhenDisconnectionUnsuccesful()
		{
			context.Current.Settings.SessionMode = SessionMode.Server;
			server.Setup(s => s.Disconnect()).Returns(new ServerResponse(false));

			var result = sut.Revert();

			fileSystem.VerifyNoOtherCalls();
			server.Verify(s => s.Disconnect(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustDoNothingIfNormalSession()
		{
			context.Current.Settings.SessionMode = SessionMode.Normal;

			var result = sut.Revert();

			repository.VerifyNoOtherCalls();
			fileSystem.VerifyNoOtherCalls();
			server.VerifyNoOtherCalls();
			uiFactory.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
