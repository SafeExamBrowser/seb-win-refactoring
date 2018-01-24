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
		private Mock<ISettingsRepository> settings;
		private Mock<ISplashScreen> splashScreen;
		private Mock<IText> text;
		private ServiceOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			service = new Mock<IServiceProxy>();
			settings = new Mock<ISettingsRepository>();
			splashScreen = new Mock<ISplashScreen>();
			text = new Mock<IText>();

			sut = new ServiceOperation(logger.Object, service.Object, settings.Object, text.Object)
			{
				SplashScreen = splashScreen.Object
			};
		}

		[TestMethod]
		public void MustConnectToService()
		{
			service.Setup(s => s.Connect()).Returns(true);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Returns(true);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			service.Verify(s => s.Connect(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotFailIfServiceNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			service.Setup(s => s.Connect()).Throws<Exception>();
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			service.Setup(s => s.Connect()).Throws<Exception>();
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
		}

		[TestMethod]
		public void MustAbortIfServiceMandatoryAndNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();

			Assert.IsTrue(sut.AbortStartup);
		}

		[TestMethod]
		public void MustNotAbortIfServiceOptionalAndNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();

			Assert.IsFalse(sut.AbortStartup);
		}

		[TestMethod]
		public void MustDisconnectWhenReverting()
		{
			service.Setup(s => s.Connect()).Returns(true);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Returns(true);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Exactly(2));
		}

		[TestMethod]
		public void MustNotDisconnnectIfNotAvailable()
		{
			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Returns(false);
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Throws<Exception>();
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Mandatory);

			sut.Perform();
			sut.Revert();

			service.Setup(s => s.Connect()).Throws<Exception>();
			settings.SetupGet(s => s.Current.ServicePolicy).Returns(ServicePolicy.Optional);

			sut.Perform();
			sut.Revert();

			service.Verify(s => s.Disconnect(), Times.Never);
		}
	}
}
