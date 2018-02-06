/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ServiceOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IServiceProxy> service;
		private Mock<IConfigurationRepository> configuration;
		private Mock<IProgressIndicator> progressIndicator;
		private Mock<IText> text;
		private ServiceOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			service = new Mock<IServiceProxy>();
			configuration = new Mock<IConfigurationRepository>();
			progressIndicator = new Mock<IProgressIndicator>();
			text = new Mock<IText>();

			sut = new ServiceOperation(configuration.Object, logger.Object, service.Object, text.Object);
		}

		[TestMethod]
		public void MustConnectToService()
		{
			service.Setup(s => s.Connect()).Returns(true);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Returns(true);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			service.Verify(s => s.Connect(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailIfServiceNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			service.Setup(s => s.Connect()).Throws<Exception>();
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Throws<Exception>();
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
		}

		[TestMethod]
		public void MustAbortIfServiceMandatoryAndNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			Assert.IsTrue(sut.Abort);
		}

		[TestMethod]
		public void MustNotAbortIfServiceOptionalAndNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			service.VerifySet(s => s.Ignore = true);
			Assert.IsFalse(sut.Abort);
		}

		[TestMethod]
		public void MustDisconnectWhenReverting()
		{
			service.Setup(s => s.Connect()).Returns(true);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Returns(true);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailWhenDisconnecting()
		{
			service.Setup(s => s.Connect()).Returns(true);
			service.Setup(s => s.Disconnect()).Throws<Exception>();
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Once);
		}

		[TestMethod]
		public void MustNotDisconnnectIfNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Returns(false);
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Throws<Exception>();
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Throws<Exception>();
			configuration.SetupGet(s => s.CurrentSettings.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Never);
		}
	}
}
