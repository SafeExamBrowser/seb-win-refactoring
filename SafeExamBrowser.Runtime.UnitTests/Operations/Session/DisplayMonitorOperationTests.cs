/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class DisplayMonitorOperationTests
	{
		private RuntimeContext context;
		private Mock<IDisplayMonitor> displayMonitor;
		private Mock<IMessageBox> messageBox;
		private AppSettings settings;
		private Mock<IText> text;

		private DisplayMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			displayMonitor = new Mock<IDisplayMonitor>();
			messageBox = new Mock<IMessageBox>();
			settings = new AppSettings();
			text = new Mock<IText>();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), context),
				Mock.Of<ILogger>(),
				messageBox.Object,
				Mock.Of<IRuntimeWindow>(),
				context,
				text.Object);

			sut = new DisplayMonitorOperation(dependencies, displayMonitor.Object);
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
			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);
			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns(string.Empty);

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
			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);
			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns(string.Empty);

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
