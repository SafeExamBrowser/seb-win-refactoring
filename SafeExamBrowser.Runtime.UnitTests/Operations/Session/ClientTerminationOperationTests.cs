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
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class ClientTerminationOperationTests
	{
		private AppConfig appConfig;
		private Mock<IClientProxy> proxy;
		private Mock<IProcess> process;
		private Mock<IProcessFactory> processFactory;
		private Mock<IProxyFactory> proxyFactory;
		private Mock<IRuntimeHost> runtimeHost;
		private SessionConfiguration session;
		private RuntimeContext runtimeContext;
		private ClientTerminationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			runtimeContext = new RuntimeContext();
			runtimeHost = new Mock<IRuntimeHost>();

			appConfig = new AppConfig();
			process = new Mock<IProcess>();
			processFactory = new Mock<IProcessFactory>();
			proxy = new Mock<IClientProxy>();
			proxyFactory = new Mock<IProxyFactory>();
			session = new SessionConfiguration();

			session.AppConfig = appConfig;
			runtimeContext.ClientProcess = process.Object;
			runtimeContext.ClientProxy = proxy.Object;
			runtimeContext.Current = session;
			runtimeContext.Next = session;
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>(), It.IsAny<Interlocutor>())).Returns(proxy.Object);

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), runtimeContext),
				Mock.Of<ILogger>(),
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				runtimeContext,
				Mock.Of<IText>());

			sut = new ClientTerminationOperation(dependencies, processFactory.Object, proxyFactory.Object, runtimeHost.Object, 0);
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
			process.Verify(p => p.TryKill(default), Times.Never);

			Assert.IsNull(runtimeContext.ClientProcess);
			Assert.IsNull(runtimeContext.ClientProxy);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRepeatIfNoClientRunning()
		{
			process.SetupGet(p => p.HasTerminated).Returns(true);
			runtimeContext.ClientProcess = process.Object;

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
