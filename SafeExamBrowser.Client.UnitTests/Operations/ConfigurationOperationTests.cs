/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ConfigurationOperationTests
	{
		private ClientConfiguration configuration;
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<IRuntimeProxy> runtime;
		private ConfigurationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			configuration = new ClientConfiguration();
			context = new ClientContext();
			logger = new Mock<ILogger>();
			runtime = new Mock<IRuntimeProxy>();

			sut = new ConfigurationOperation(configuration, context, logger.Object, runtime.Object);
		}

		[TestMethod]
		public void MustCorrectlySetConfiguration()
		{
			var response = new ConfigurationResponse
			{
				Configuration = new ClientConfiguration
				{
					AppConfig = new AppConfig(),
					SessionId = Guid.NewGuid(),
					Settings = new AppSettings()
				}
			};

			runtime.Setup(r => r.GetConfiguration()).Returns(new CommunicationResult<ConfigurationResponse>(true, response));

			var result = sut.Perform();

			Assert.AreSame(configuration.AppConfig, response.Configuration.AppConfig);
			Assert.AreEqual(configuration.SessionId, response.Configuration.SessionId);
			Assert.AreSame(configuration.Settings, response.Configuration.Settings);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			var result = sut.Revert();

			logger.VerifyNoOtherCalls();
			runtime.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
