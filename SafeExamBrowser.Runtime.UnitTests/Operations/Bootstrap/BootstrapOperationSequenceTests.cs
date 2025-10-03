/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Bootstrap;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Bootstrap
{
	[TestClass]
	public class BootstrapOperationSequenceTests
	{
		private Mock<ILogger> logger;
		private Mock<IOperation> operationA;
		private Mock<IOperation> operationB;
		private IOperation[] operations;
		private Mock<ISplashScreen> splashScreen;

		private BootstrapOperationSequence sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			operationA = new Mock<IOperation>();
			operationB = new Mock<IOperation>();
			operations = new[] { operationA.Object, operationB.Object };
			splashScreen = new Mock<ISplashScreen>();

			sut = new BootstrapOperationSequence(logger.Object, operations, splashScreen.Object);
		}

		[TestMethod]
		public void MustUpdateProgress()
		{
			operationA.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationA.Setup(o => o.Revert()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Perform()).Returns(OperationResult.Success);
			operationB.Setup(o => o.Revert()).Returns(OperationResult.Success);

			sut.TryPerform();
			sut.TryRevert();

			splashScreen.Verify(s => s.SetValue(It.Is<int>(v => v == 0)), Times.Once);
			splashScreen.Verify(s => s.SetIndeterminate(), Times.Once);
			splashScreen.Verify(s => s.SetMaxValue(It.Is<int>(v => v == 2)), Times.Once);
			splashScreen.Verify(s => s.Progress(), Times.Exactly(2));

			operationA.Setup(o => o.Perform()).Returns(OperationResult.Failed);

			sut.TryPerform();

			splashScreen.Verify(s => s.Regress(), Times.Once);
		}

		[TestMethod]
		public void MustUpdateStatus()
		{
			operationA.Raise(o => o.StatusChanged += default, TextKey.Build);
			operationB.Raise(o => o.StatusChanged += default, TextKey.Version);

			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(t => t == TextKey.Build), true), Times.Once);
			splashScreen.Verify(s => s.UpdateStatus(It.Is<TextKey>(t => t == TextKey.Version), true), Times.Once);
		}
	}
}
