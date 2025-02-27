/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class ServerResponsibilityTests
	{
		private ClientContext context;
		private Mock<ICoordinator> coordinator;
		private Mock<IRuntimeProxy> runtime;
		private Mock<IServerProxy> server;
		private Mock<IText> text;

		private ServerResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();
			var responsibilities = new Mock<IResponsibilityCollection<ClientTask>>();

			context = new ClientContext();
			coordinator = new Mock<ICoordinator>();
			runtime = new Mock<IRuntimeProxy>();
			server = new Mock<IServerProxy>();
			text = new Mock<IText>();

			context.Responsibilities = responsibilities.Object;
			context.Runtime = runtime.Object;
			context.Server = server.Object;

			sut = new ServerResponsibility(context, coordinator.Object, logger.Object, text.Object);
			sut.Assume(ClientTask.RegisterEvents);
		}

		[TestMethod]
		public void Server_MustInitiateShutdownOnEvent()
		{
			runtime.Setup(r => r.RequestShutdown()).Returns(new CommunicationResult(true));

			server.Raise(s => s.TerminationRequested += null);

			runtime.Verify(p => p.RequestShutdown(), Times.Once);
		}

		[TestMethod]
		public void MustNotFailIfDependencyIsNull()
		{
			context.Server = null;
			sut.Assume(ClientTask.DeregisterEvents);
		}
	}
}
