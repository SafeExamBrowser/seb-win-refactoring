/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class IntegrityResponsibilityTests
	{
		private Mock<IText> text;
		private Mock<IIntegrityModule> integrityModule;

		[TestInitialize]
		public void Initialize()
		{
			var context = new ClientContext();
			var logger = new Mock<ILogger>();
			var valid = true;

			text = new Mock<IText>();
			integrityModule = new Mock<IIntegrityModule>();

			integrityModule.Setup(m => m.TryVerifySessionIntegrity(It.IsAny<string>(), It.IsAny<string>(), out valid)).Returns(true);

			var sut = new IntegrityResponsibility(context, logger.Object, text.Object);
		}
	}
}
