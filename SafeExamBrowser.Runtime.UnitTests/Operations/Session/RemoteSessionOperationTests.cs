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
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class RemoteSessionOperationTests
	{
		private RuntimeContext context;
		private Mock<IRemoteSessionDetector> detector;
		private Mock<IMessageBox> messageBox;
		private AppSettings settings;

		private RemoteSessionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new RuntimeContext();
			detector = new Mock<IRemoteSessionDetector>();
			messageBox = new Mock<IMessageBox>();
			settings = new AppSettings();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), context),
				Mock.Of<ILogger>(),
				messageBox.Object,
				Mock.Of<IRuntimeWindow>(),
				context,
				Mock.Of<IText>());

			sut = new RemoteSessionOperation(dependencies, detector.Object);
		}

		[TestMethod]
		public void Perform_MustAbortIfRemoteSessionNotAllowed()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = true;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Perform();

			detector.Verify(d => d.IsRemoteSession(), Times.Once);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfRemoteSessionAllowed()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = false;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Perform();

			detector.Verify(d => d.IsRemoteSession(), Times.AtMostOnce);

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSucceedIfNoRemoteSession()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(false);
			settings.Service.DisableRemoteConnections = true;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Perform();

			detector.Verify(d => d.IsRemoteSession(), Times.Once);

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustAbortIfRemoteSessionNotAllowed()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = true;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Repeat();

			detector.Verify(d => d.IsRemoteSession(), Times.Once);

			Assert.IsTrue(messageShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedIfRemoteSessionAllowed()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = false;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Repeat();

			detector.Verify(d => d.IsRemoteSession(), Times.AtMostOnce);

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedIfNoRemoteSession()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(false);
			settings.Service.DisableRemoteConnections = true;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Repeat();

			detector.Verify(d => d.IsRemoteSession(), Times.Once);

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDoNoting()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = false;

			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Callback(() => messageShown = true);

			var result = sut.Revert();

			detector.VerifyNoOtherCalls();

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
