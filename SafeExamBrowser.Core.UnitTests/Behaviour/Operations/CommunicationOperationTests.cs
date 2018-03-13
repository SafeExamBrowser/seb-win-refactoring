/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Behaviour.OperationModel;

namespace SafeExamBrowser.Core.UnitTests.Behaviour.Operations
{
	[TestClass]
	public class CommunicationOperationTests
	{
		private Mock<ICommunicationHost> hostMock;
		private Mock<ILogger> loggerMock;
		private CommunicationOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			hostMock = new Mock<ICommunicationHost>();
			loggerMock = new Mock<ILogger>();

			sut = new CommunicationOperation(hostMock.Object, loggerMock.Object);
		}

		[TestMethod]
		public void MustRestartHostOnRepeat()
		{
			var order = 0;
			var stop = 0;
			var start = 0;

			hostMock.Setup(h => h.Stop()).Callback(() => stop = ++order);
			hostMock.Setup(h => h.Start()).Callback(() => start = ++order);

			sut.Repeat();

			hostMock.Verify(h => h.Stop(), Times.Once);
			hostMock.Verify(h => h.Start(), Times.Once);

			Assert.AreEqual(stop, 1);
			Assert.AreEqual(start, 2);
		}

		[TestMethod]
		public void MustStartHostOnPerform()
		{
			sut.Perform();

			hostMock.Verify(h => h.Start(), Times.Once);
			hostMock.Verify(h => h.Stop(), Times.Never);
		}

		[TestMethod]
		public void MustStopHostOnRevert()
		{
			sut.Revert();

			hostMock.Verify(h => h.Stop(), Times.Once);
			hostMock.Verify(h => h.Start(), Times.Never);
		}
	}
}
