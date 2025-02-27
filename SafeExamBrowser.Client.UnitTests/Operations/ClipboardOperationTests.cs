/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ClipboardOperationTests
	{
		private Mock<IClipboard> clipboard;
		private ClientContext context;
		private Mock<ILogger> loggerMock;

		private ClipboardOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			clipboard = new Mock<IClipboard>();
			context = new ClientContext();
			context.Settings = new AppSettings();
			loggerMock = new Mock<ILogger>();

			sut = new ClipboardOperation(context, clipboard.Object, loggerMock.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			sut.Perform();
			clipboard.Verify(n => n.Initialize(It.IsAny<ClipboardPolicy>()), Times.Once);
		}

		[TestMethod]
		public void MustRevertCorrectly()
		{
			sut.Revert();
			clipboard.Verify(n => n.Terminate(), Times.Once);
		}
	}
}
