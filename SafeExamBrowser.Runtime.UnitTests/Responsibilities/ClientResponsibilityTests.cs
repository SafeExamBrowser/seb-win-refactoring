/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class ClientResponsibilityTests
	{
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private RuntimeContext context;
		private Mock<Action> shutdown;
		private Mock<IProcess> clientProcess;
		private Mock<IClientProxy> clientProxy;

		private ClientResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			clientProcess = new Mock<IProcess>();
			clientProxy = new Mock<IClientProxy>();
			context = new RuntimeContext();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			shutdown = new Mock<Action>();

			context.ClientProcess = clientProcess.Object;
			context.ClientProxy = clientProxy.Object;
			context.Current = new SessionConfiguration();
			context.Responsibilities = responsibilities.Object;

			sut = new ClientResponsibility(logger.Object, messageBox.Object, context, runtimeWindow.Object, shutdown.Object);
		}

		[TestMethod]
		public void ClientProcess_MustShutdownWhenClientTerminated()
		{
			sut.Assume(RuntimeTask.RegisterSessionEvents);
			clientProcess.Raise(c => c.Terminated += null, -1);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StopSession), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void ClientProxy_MustShutdownWhenConnectionLost()
		{
			sut.Assume(RuntimeTask.RegisterSessionEvents);
			clientProxy.Raise(c => c.ConnectionLost += null);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Once);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StopSession), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void MustCorrectlyDeregisterEvents()
		{
			sut.Assume(RuntimeTask.DeregisterSessionEvents);

			clientProcess.Raise(c => c.Terminated += null);
			clientProxy.Raise(c => c.ConnectionLost += null);

			messageBox.VerifyNoOtherCalls();
			responsibilities.VerifyNoOtherCalls();
			shutdown.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustNotFailToDeregisterEventsIfDependenciesNull()
		{
			context.ClientProcess = null;
			context.ClientProxy = null;

			sut.Assume(RuntimeTask.DeregisterSessionEvents);
		}
	}
}
