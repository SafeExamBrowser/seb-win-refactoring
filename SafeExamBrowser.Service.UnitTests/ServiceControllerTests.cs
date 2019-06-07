/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;

namespace SafeExamBrowser.Service.UnitTests
{
	[TestClass]
	public class ServiceControllerTests
	{
		private Mock<IOperationSequence> bootstrapSequence;
		private SessionContext sessionContext;
		private Mock<IRepeatableOperationSequence> sessionSequence;
		private Mock<IServiceHost> serviceHost;
		private ServiceController sut;

		[TestInitialize]
		public void Initialize()
		{
			bootstrapSequence = new Mock<IOperationSequence>();
			sessionContext = new SessionContext();
			sessionSequence = new Mock<IRepeatableOperationSequence>();
			serviceHost = new Mock<IServiceHost>();

			sut = new ServiceController(bootstrapSequence.Object, sessionSequence.Object, serviceHost.Object, sessionContext);
		}

		[TestMethod]
		public void Start_MustOnlyPerformBootstrapSequence()
		{
			bootstrapSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Success);
			sessionContext.Current = null;

			var success = sut.TryStart();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Once);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Never);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.IsTrue(success);
		}

		[TestMethod]
		public void Stop_MustRevertSessionThenBootstrapSequence()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Once);

			Assert.AreEqual(1, session);
			Assert.AreEqual(2, bootstrap);
		}

		[TestMethod]
		public void Stop_MustNotRevertSessionSequenceIfNoSessionRunning()
		{
			var order = 0;
			var bootstrap = 0;
			var session = 0;

			sut.TryStart();

			bootstrapSequence.Reset();
			sessionSequence.Reset();

			bootstrapSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => bootstrap = ++order);
			sessionSequence.Setup(b => b.TryRevert()).Returns(OperationResult.Success).Callback(() => session = ++order);
			sessionContext.Current = null;

			sut.Terminate();

			bootstrapSequence.Verify(b => b.TryPerform(), Times.Never);
			bootstrapSequence.Verify(b => b.TryRevert(), Times.Once);
			sessionSequence.Verify(b => b.TryPerform(), Times.Never);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);

			Assert.AreEqual(0, session);
			Assert.AreEqual(1, bootstrap);
		}
	}
}
