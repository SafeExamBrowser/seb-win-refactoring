/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Service.Operations;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class RestoreOperationTests
	{
		private SessionContext sessionContext;
		private Mock<IAutoRestoreMechanism> autoRestoreMechanism;
		private Mock<IFeatureConfigurationBackup> backup;
		private Mock<ILogger> logger;
		private RestoreOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			autoRestoreMechanism = new Mock<IAutoRestoreMechanism>();
			backup = new Mock<IFeatureConfigurationBackup>();
			logger = new Mock<ILogger>();
			sessionContext = new SessionContext { AutoRestoreMechanism = autoRestoreMechanism.Object };

			sut = new RestoreOperation(backup.Object, logger.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustStartAutoRestore()
		{
			var result = sut.Perform();

			autoRestoreMechanism.Verify(m => m.Start(), Times.Once);
			autoRestoreMechanism.Verify(m => m.Stop(), Times.Never);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustStopAutoRestore()
		{
			var result = sut.Revert();

			autoRestoreMechanism.Verify(m => m.Start(), Times.Never);
			autoRestoreMechanism.Verify(m => m.Stop(), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
