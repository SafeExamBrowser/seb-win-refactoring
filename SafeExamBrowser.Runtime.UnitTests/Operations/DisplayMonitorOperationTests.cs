/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Monitoring;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class DisplayMonitorOperationTests
	{
		private SessionContext context;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<ILogger> logger;
		private AppSettings settings;
		private Mock<IText> text;

		private DisplayMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new SessionContext();
			displayMonitor = new Mock<IDisplayMonitor>();
			logger = new Mock<ILogger>();
			settings = new AppSettings();
			text = new Mock<IText>();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;
			sut = new DisplayMonitorOperation(displayMonitor.Object, logger.Object, context, text.Object);
		}

		[TestMethod]
		public void Perform_MustSucceedIfDisplayConfigurationAllowed()
		{
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });

			var result = sut.Perform();

			displayMonitor.Verify(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailIfDisplayConfigurationNotAllowed()
		{
			var messageShown = false;

			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = false });
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

			var result = sut.Perform();

			displayMonitor.Verify(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>()), Times.Once);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedIfDisplayConfigurationAllowed()
		{
			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = true });

			var result = sut.Repeat();

			displayMonitor.Verify(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>()), Times.Once);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustFailIfDisplayConfigurationNotAllowed()
		{
			var messageShown = false;

			displayMonitor.Setup(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>())).Returns(new ValidationResult { IsAllowed = false });
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

			var result = sut.Repeat();

			displayMonitor.Verify(m => m.ValidateConfiguration(It.IsAny<DisplaySettings>()), Times.Once);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var result = sut.Revert();

			displayMonitor.VerifyNoOtherCalls();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
