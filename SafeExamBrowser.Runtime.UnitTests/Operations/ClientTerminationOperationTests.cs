/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
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
		private SessionConfiguration session;
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
			session = new SessionConfiguration();
			sessionContext = new SessionContext();
			terminated = new Action(() =>
			{
				runtimeHost.Raise(h => h.ClientDisconnected += null);
				process.Raise(p => p.Terminated += null, 0);
			});

			session.AppConfig = appConfig;
			sessionContext.ClientProcess = process.Object;
			sessionContext.ClientProxy = proxy.Object;
			sessionContext.Current = session;
			sessionContext.Next = session;
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>(), It.IsAny<Interlocutor>())).Returns(proxy.Object);

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
			process.Verify(p => p.TryKill(default(int)), Times.Never);

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

		[TestMethod]
		public void MustDoNothingOnPerform()
		{
			var result = sut.Perform();

			process.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
			proxy.VerifyNoOtherCalls();
			proxyFactory.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			var result = sut.Revert();

			process.VerifyNoOtherCalls();
			processFactory.VerifyNoOtherCalls();
			proxy.VerifyNoOtherCalls();
			proxyFactory.VerifyNoOtherCalls();
			runtimeHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
