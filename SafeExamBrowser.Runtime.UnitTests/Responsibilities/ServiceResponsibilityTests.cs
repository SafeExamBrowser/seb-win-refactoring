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
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class ServiceResponsibilityTests
	{
		private RuntimeContext context;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<IServiceProxy> serviceProxy;
		private Mock<Action> shutdown;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private ServiceResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			serviceProxy = new Mock<IServiceProxy>();
			shutdown = new Mock<Action>();

			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			currentSession.Settings = currentSettings;

			context.Current = currentSession;
			context.Responsibilities = responsibilities.Object;

			sut = new ServiceResponsibility(logger.Object, messageBox.Object, context, runtimeWindow.Object, serviceProxy.Object, shutdown.Object);
		}

		[TestMethod]
		public void ServiceProxy_MustShutdownWhenConnectionLostAndMandatory()
		{
			currentSettings.Service.Policy = ServicePolicy.Mandatory;

			sut.Assume(RuntimeTask.RegisterSessionEvents);
			serviceProxy.Raise(c => c.ConnectionLost += null);

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
		public void ServiceProxy_MustNotShutdownWhenConnectionLostAndNotMandatory()
		{
			currentSettings.Service.Policy = ServicePolicy.Optional;

			sut.Assume(RuntimeTask.RegisterSessionEvents);
			serviceProxy.Raise(c => c.ConnectionLost += null);

			messageBox.Verify(m => m.Show(
				It.IsAny<TextKey>(),
				It.IsAny<TextKey>(),
				It.IsAny<MessageBoxAction>(),
				It.Is<MessageBoxIcon>(i => i == MessageBoxIcon.Error),
				It.IsAny<IWindow>()), Times.Never);
			responsibilities.Verify(r => r.Delegate(RuntimeTask.StopSession), Times.Never);
			shutdown.Verify(s => s(), Times.Never);
		}
	}
}
