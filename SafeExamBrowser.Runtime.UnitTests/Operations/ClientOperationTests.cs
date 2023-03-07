/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Settings;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ClientOperationTests
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
		private AppSettings settings;
		private ClientOperation sut;

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
			settings = new AppSettings();
			terminated = new Action(() =>
			{
				runtimeHost.Raise(h => h.ClientDisconnected += null);
				process.Raise(p => p.Terminated += null, 0);
			});

			appConfig.ClientLogFilePath = "";
			session.AppConfig = appConfig;
			session.Settings = settings;
			sessionContext.Current = session;
			sessionContext.Next = session;
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>(), It.IsAny<Interlocutor>())).Returns(proxy.Object);

			sut = new ClientOperation(logger.Object, processFactory.Object, proxyFactory.Object, runtimeHost.Object, sessionContext, 0);
		}

		[TestMethod]
		public void Perform_MustStartClient()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Perform();

			Assert.AreEqual(process.Object, sessionContext.ClientProcess);
			Assert.AreEqual(proxy.Object, sessionContext.ClientProxy);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailStartupImmediatelyIfClientTerminates()
		{
			const int ONE_SECOND = 1000;

			var after = default(DateTime);
			var before = default(DateTime);
			var result = default(OperationResult);
			var terminateClient = new Action(() => Task.Delay(100).ContinueWith(_ => process.Raise(p => p.Terminated += null, 0)));

			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(terminateClient);
			sut = new ClientOperation(logger.Object, processFactory.Object, proxyFactory.Object, runtimeHost.Object, sessionContext, ONE_SECOND);

			before = DateTime.Now;
			result = sut.Perform();
			after = DateTime.Now;

			Assert.IsTrue(after - before < new TimeSpan(0, 0, ONE_SECOND));
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustFailStartupIfConnectionToClientNotEstablished()
		{
			var result = default(OperationResult);

			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(false);

			result = sut.Perform();

			Assert.IsNotNull(sessionContext.ClientProcess);
			Assert.IsNotNull(sessionContext.ClientProxy);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustFailStartupIfAuthenticationNotSuccessful()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = -1 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(1234);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Perform();

			Assert.IsNotNull(sessionContext.ClientProcess);
			Assert.IsNotNull(sessionContext.ClientProxy);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustStartClient()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Repeat();

			Assert.AreEqual(process.Object, sessionContext.ClientProcess);
			Assert.AreEqual(proxy.Object, sessionContext.ClientProxy);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustStopClient()
		{
			proxy.Setup(p => p.Disconnect()).Callback(terminated);

			PerformNormally();
			sut.Revert();

			proxy.Verify(p => p.InitiateShutdown(), Times.Once);
			proxy.Verify(p => p.Disconnect(), Times.Once);
			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.Never);

			Assert.IsNull(sessionContext.ClientProcess);
			Assert.IsNull(sessionContext.ClientProxy);
		}

		[TestMethod]
		public void Revert_MustKillClientIfStoppingFailed()
		{
			process.Setup(p => p.TryKill(It.IsAny<int>())).Callback(() => process.SetupGet(p => p.HasTerminated).Returns(true));

			PerformNormally();
			sut.Revert();

			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.AtLeastOnce);

			Assert.IsNull(sessionContext.ClientProcess);
			Assert.IsNull(sessionContext.ClientProxy);
		}

		[TestMethod]
		public void Revert_MustAttemptToKillFiveTimesThenAbort()
		{
			PerformNormally();
			sut.Revert();

			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.Exactly(5));

			Assert.IsNotNull(sessionContext.ClientProcess);
			Assert.IsNotNull(sessionContext.ClientProxy);
		}

		[TestMethod]
		public void Revert_MustNotStopClientIfAlreadyTerminated()
		{
			process.SetupGet(p => p.HasTerminated).Returns(true);

			sut.Revert();

			proxy.Verify(p => p.InitiateShutdown(), Times.Never);
			proxy.Verify(p => p.Disconnect(), Times.Never);
			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.Never);

			Assert.IsNull(sessionContext.ClientProcess);
			Assert.IsNull(sessionContext.ClientProxy);
		}

		private void PerformNormally()
		{
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			sut.Perform();
		}
	}
}
