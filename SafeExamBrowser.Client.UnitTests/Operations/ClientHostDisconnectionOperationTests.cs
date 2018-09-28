/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ClientHostDisconnectionOperationTests
	{
		private Mock<IClientHost> clientHost;
		private Mock<ILogger> logger;

		private ClientHostDisconnectionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			clientHost = new Mock<IClientHost>();
			logger = new Mock<ILogger>();

			sut = new ClientHostDisconnectionOperation(clientHost.Object, logger.Object, 0);
		}

		[TestMethod]
		public void MustWaitForDisconnectionIfConnectionIsActive()
		{
			var stopWatch = new Stopwatch();
			var timeout_ms = 1000;

			sut = new ClientHostDisconnectionOperation(clientHost.Object, logger.Object, timeout_ms);

			clientHost.SetupGet(h => h.IsConnected).Returns(true).Callback(() => clientHost.Raise(h => h.RuntimeDisconnected += null));

			stopWatch.Start();
			sut.Revert();
			stopWatch.Stop();

			clientHost.VerifyGet(h => h.IsConnected);
			clientHost.VerifyNoOtherCalls();

			Assert.IsFalse(stopWatch.IsRunning);
			Assert.IsTrue(stopWatch.ElapsedMilliseconds < timeout_ms);
		}

		[TestMethod]
		public void MustRespectTimeoutIfWaitingForDisconnection()
		{
			var stopWatch = new Stopwatch();
			var timeout_ms = 50;

			sut = new ClientHostDisconnectionOperation(clientHost.Object, logger.Object, timeout_ms);

			clientHost.SetupGet(h => h.IsConnected).Returns(true);

			stopWatch.Start();
			sut.Revert();
			stopWatch.Stop();

			clientHost.VerifyGet(h => h.IsConnected);
			clientHost.VerifyNoOtherCalls();

			Assert.IsFalse(stopWatch.IsRunning);
			Assert.IsTrue(stopWatch.ElapsedMilliseconds > timeout_ms);
		}

		[TestMethod]
		public void MustDoNothingIfNoConnectionIsActive()
		{
			clientHost.SetupGet(h => h.IsConnected).Returns(false);

			sut.Revert();

			clientHost.VerifyGet(h => h.IsConnected);
			clientHost.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustDoNothingOnPerform()
		{
			var result = sut.Perform();

			clientHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRepeat()
		{
			var result = sut.Repeat();

			clientHost.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
