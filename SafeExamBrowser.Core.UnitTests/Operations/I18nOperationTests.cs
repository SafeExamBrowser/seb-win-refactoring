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
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.UnitTests.Operations
{
	[TestClass]
	public class I18nOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IText> text;

		private I18nOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			text = new Mock<IText>();

			sut = new I18nOperation(logger.Object, text.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var result = sut.Perform();

			text.Verify(t => t.Initialize(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustDoNothingOnRevert()
		{
			sut.Revert();
			text.VerifyNoOtherCalls();
		}
	}
}
