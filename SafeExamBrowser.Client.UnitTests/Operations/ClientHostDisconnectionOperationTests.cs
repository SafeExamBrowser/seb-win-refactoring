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
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ClientHostDisconnectionOperationTests
	{
		private Mock<IClientHost> clientHost;
		private ClientContext context;
		private Mock<ILogger> logger;

		private ClientHostDisconnectionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			clientHost = new Mock<IClientHost>();
			context = new ClientContext();
			logger = new Mock<ILogger>();

			context.ClientHost = clientHost.Object;

			sut = new ClientHostDisconnectionOperation(context, logger.Object, 0);
		}

		[TestMethod]
		public void MustWaitForDisconnectionIfConnectionIsActive()
		{
			var after = default(DateTime);
			var before = default(DateTime);
			var timeout_ms = 200;

			sut = new ClientHostDisconnectionOperation(context, logger.Object, timeout_ms);

			clientHost.SetupGet(h => h.IsConnected).Returns(true).Callback(() => Task.Delay(10).ContinueWith((_) =>
			{
				clientHost.Raise(h => h.RuntimeDisconnected += null);
			}));

			before = DateTime.Now;
			sut.Revert();
			after = DateTime.Now;

			clientHost.VerifyGet(h => h.IsConnected);
			clientHost.VerifyAdd(h => h.RuntimeDisconnected += It.IsAny<CommunicationEventHandler>());
			clientHost.VerifyRemove(h => h.RuntimeDisconnected -= It.IsAny<CommunicationEventHandler>());
			clientHost.VerifyNoOtherCalls();

			Assert.IsTrue(after - before < new TimeSpan(0, 0, 0, 0, timeout_ms));
		}

		[TestMethod]
		public void MustRespectTimeoutIfWaitingForDisconnection()
		{
			var after = default(DateTime);
			var before = default(DateTime);
			var timeout_ms = 200;

			sut = new ClientHostDisconnectionOperation(context, logger.Object, timeout_ms);

			clientHost.SetupGet(h => h.IsConnected).Returns(true);

			before = DateTime.Now;
			sut.Revert();
			after = DateTime.Now;

			clientHost.VerifyGet(h => h.IsConnected);
			clientHost.VerifyAdd(h => h.RuntimeDisconnected += It.IsAny<CommunicationEventHandler>());
			clientHost.VerifyRemove(h => h.RuntimeDisconnected -= It.IsAny<CommunicationEventHandler>());
			clientHost.VerifyNoOtherCalls();

			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, timeout_ms));
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
	}
}
