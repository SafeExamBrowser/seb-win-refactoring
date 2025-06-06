/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class CommunicationResponsibilityTests
	{
		private Mock<IClientProxy> clientProxy;
		private RuntimeContext context;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private Mock<ILogger> logger;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<Action> shutdown;

		private CommunicationResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			clientProxy = new Mock<IClientProxy>();
			context = new RuntimeContext();
			logger = new Mock<ILogger>();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();
			runtimeHost = new Mock<IRuntimeHost>();
			shutdown = new Mock<Action>();

			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;

			context.ClientProxy = clientProxy.Object;
			context.Current = currentSession;
			context.Next = nextSession;
			context.Responsibilities = responsibilities.Object;

			sut = new CommunicationResponsibility(logger.Object, context, runtimeHost.Object, shutdown.Object);
		}

		[TestMethod]
		public void Communication_MustProvideClientConfigurationUponRequest()
		{
			var args = new ClientConfigurationEventArgs();
			var nextAppConfig = new AppConfig();
			var nextSessionId = Guid.NewGuid();
			var nextSettings = new AppSettings();

			nextSession.AppConfig = nextAppConfig;
			nextSession.SessionId = nextSessionId;
			nextSession.Settings = nextSettings;

			sut.Assume(RuntimeTask.RegisterEvents);
			runtimeHost.Raise(r => r.ClientConfigurationNeeded += null, args);

			Assert.AreSame(nextAppConfig, args.ClientConfiguration.AppConfig);
			Assert.AreEqual(nextSessionId, args.ClientConfiguration.SessionId);
			Assert.AreSame(nextSettings, args.ClientConfiguration.Settings);
		}

		[TestMethod]
		public void Communication_MustStartNewSessionUponRequest()
		{
			var args = new ReconfigurationEventArgs { ConfigurationPath = "C:\\Some\\File\\Path.seb" };

			sut.Assume(RuntimeTask.RegisterEvents);
			runtimeHost.Raise(r => r.ReconfigurationRequested += null, args);

			responsibilities.Verify(r => r.Delegate(RuntimeTask.StartSession));
			Assert.AreEqual(context.ReconfigurationFilePath, args.ConfigurationPath);
		}

		[TestMethod]
		public void Communication_MustShutdownUponRequest()
		{
			sut.Assume(RuntimeTask.RegisterEvents);
			runtimeHost.Raise(r => r.ShutdownRequested += null);
			shutdown.Verify(s => s(), Times.Once);
		}
	}
}
