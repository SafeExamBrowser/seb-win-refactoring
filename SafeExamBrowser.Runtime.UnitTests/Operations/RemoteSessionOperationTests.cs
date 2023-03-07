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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class RemoteSessionOperationTests
	{
		private SessionContext context;
		private Mock<IRemoteSessionDetector> detector;
		private Mock<ILogger> logger;
		private AppSettings settings;

		private RemoteSessionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new SessionContext();
			detector = new Mock<IRemoteSessionDetector>();
			logger = new Mock<ILogger>();
			settings = new AppSettings();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;
			sut = new RemoteSessionOperation(detector.Object, logger.Object, context);
		}

		[TestMethod]
		public void Perform_MustAbortIfRemoteSessionNotAllowed()
		{
			var messageShown = false;

			detector.Setup(d => d.IsRemoteSession()).Returns(true);
			settings.Service.DisableRemoteConnections = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

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
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs)
				{
					messageShown = true;
				}
			};

			var result = sut.Revert();

			detector.VerifyNoOtherCalls();

			Assert.IsFalse(messageShown);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
