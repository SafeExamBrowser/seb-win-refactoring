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
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ClientTerminationOperationTests
	{
		private Action clientReady;
		private Action terminated;
		private AppConfig appConfig;
		private Mock<IClientProxy> proxy;
		private Mock<ILogger> logger;
		private Mock<IProcess> process;
		private Mock<IProcessFactory> processFactory;
		private Mock<IProxyFactory> proxyFactory;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<ISessionConfiguration> session;
		private SessionContext sessionContext;

		private ClientTerminationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			clientReady = new Action(() => runtimeHost.Raise(h => h.ClientReady += null));
			logger = new Mock<ILogger>();
			process = new Mock<IProcess>();
			processFactory = new Mock<IProcessFactory>();
			proxy = new Mock<IClientProxy>();
			proxyFactory = new Mock<IProxyFactory>();
			runtimeHost = new Mock<IRuntimeHost>();
			session = new Mock<ISessionConfiguration>();
			sessionContext = new SessionContext();
			terminated = new Action(() =>
			{
				runtimeHost.Raise(h => h.ClientDisconnected += null);
				process.Raise(p => p.Terminated += null, 0);
			});

			session.SetupGet(s => s.AppConfig).Returns(appConfig);
			sessionContext.ClientProcess = process.Object;
			sessionContext.ClientProxy = proxy.Object;
			sessionContext.Current = session.Object;
			sessionContext.Next = session.Object;
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>())).Returns(proxy.Object);

			sut = new ClientTerminationOperation(logger.Object, processFactory.Object, proxyFactory.Object, runtimeHost.Object, sessionContext, 0);
		}

		[TestMethod]
		public void MustTerminateClientOnRepeat()
		{
			var terminated = new Action(() =>
			{
				runtimeHost.Raise(h => h.ClientDisconnected += null);
				process.Raise(p => p.Terminated += null, 0);
			});

			proxy.Setup(p => p.Disconnect()).Callback(terminated);

			var result = sut.Repeat();

			proxy.Verify(p => p.InitiateShutdown(), Times.Once);
			proxy.Verify(p => p.Disconnect(), Times.Once);
			process.Verify(p => p.Kill(), Times.Never);

			Assert.IsNull(sessionContext.ClientProcess);
			Assert.IsNull(sessionContext.ClientProxy);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRepeatIfNoClientRunning()
		{
			process.SetupGet(p => p.HasTerminated).Returns(true);
			sessionContext.ClientProcess = process.Object;

			var result = sut.Repeat();

			process.VerifyGet(p => p.HasTerminated, Times.Once);

			process.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
			proxy.VerifyNoOtherCalls();
			proxyFactory.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
