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
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Behaviour.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class ClientTerminationOperationTests
	{
		private Action clientReady;
		private Mock<IConfigurationRepository> configuration;
		private Mock<IClientProxy> proxy;
		private Mock<ILogger> logger;
		private Mock<IProcess> process;
		private Mock<IProcessFactory> processFactory;
		private Mock<IProxyFactory> proxyFactory;
		private Mock<IRuntimeHost> runtimeHost;
		private RuntimeInfo runtimeInfo;
		private Mock<ISessionData> session;
		private ClientTerminationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			configuration = new Mock<IConfigurationRepository>();
			clientReady = new Action(() => runtimeHost.Raise(h => h.ClientReady += null));
			logger = new Mock<ILogger>();
			process = new Mock<IProcess>();
			processFactory = new Mock<IProcessFactory>();
			proxy = new Mock<IClientProxy>();
			proxyFactory = new Mock<IProxyFactory>();
			runtimeHost = new Mock<IRuntimeHost>();
			runtimeInfo = new RuntimeInfo();
			session = new Mock<ISessionData>();

			configuration.SetupGet(c => c.CurrentSession).Returns(session.Object);
			configuration.SetupGet(c => c.RuntimeInfo).Returns(runtimeInfo);
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>())).Returns(proxy.Object);

			sut = new ClientTerminationOperation(configuration.Object, logger.Object, processFactory.Object, proxyFactory.Object, runtimeHost.Object, 0);
		}

		[TestMethod]
		public void MustDoNothingOnPerform()
		{
			sut.Perform();

			processFactory.VerifyNoOtherCalls();
			proxy.VerifyNoOtherCalls();
			proxyFactory.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			sut.Revert();

			process.VerifyNoOtherCalls();
			proxy.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustStartClientOnRepeat()
		{
			// TODO: Extract static fields from operation -> allows unit testing of this requirement etc.!
			Assert.Fail();
		}
	}
}
