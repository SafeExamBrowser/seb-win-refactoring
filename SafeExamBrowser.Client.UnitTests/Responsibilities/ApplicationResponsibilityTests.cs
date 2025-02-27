/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class ApplicationResponsibilityTests
	{
		private ClientContext context;
		private ApplicationsResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			var logger = new Mock<ILogger>();

			context = new ClientContext();
			sut = new ApplicationsResponsibility(context, logger.Object);
		}

		[TestMethod]
		public void MustAutoStartApplications()
		{
			var application1 = new Mock<IApplication<IApplicationWindow>>();
			var application2 = new Mock<IApplication<IApplicationWindow>>();
			var application3 = new Mock<IApplication<IApplicationWindow>>();

			application1.SetupGet(a => a.AutoStart).Returns(true);
			application2.SetupGet(a => a.AutoStart).Returns(false);
			application3.SetupGet(a => a.AutoStart).Returns(true);
			context.Applications.Add(application1.Object);
			context.Applications.Add(application2.Object);
			context.Applications.Add(application3.Object);

			sut.Assume(ClientTask.AutoStartApplications);

			application1.Verify(a => a.Start(), Times.Once);
			application2.Verify(a => a.Start(), Times.Never);
			application3.Verify(a => a.Start(), Times.Once);
		}
	}
}
