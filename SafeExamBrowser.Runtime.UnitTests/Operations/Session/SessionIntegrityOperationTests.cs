/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class SessionIntegrityOperationTests
	{
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private Mock<ILogger> logger;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private RuntimeContext runtimeContext;
		private Mock<ISystemSentinel> sentinel;

		private SessionIntegrityOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			logger = new Mock<ILogger>();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();
			runtimeContext = new RuntimeContext();
			sentinel = new Mock<ISystemSentinel>();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;
			runtimeContext.Current = currentSession;
			runtimeContext.Next = nextSession;

			var dependencies = new Dependencies(
				new ClientBridge(Mock.Of<IRuntimeHost>(), runtimeContext),
				logger.Object,
				Mock.Of<IMessageBox>(),
				Mock.Of<IRuntimeWindow>(),
				runtimeContext,
				Mock.Of<IText>());

			sut = new SessionIntegrityOperation(dependencies, sentinel.Object);

			sentinel.Setup(s => s.DisableStickyKeys()).Returns(true);
			sentinel.Setup(s => s.RevertStickyKeys()).Returns(true);
			sentinel.Setup(s => s.VerifyCursors()).Returns(true);
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(true);
		}

		[TestMethod]
		public void Perform_MustSucceedWhenAllChecksSucceed()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Security.AllowStickyKeys = false;
			nextSettings.Security.VerifyCursorConfiguration = true;
			nextSettings.Service.IgnoreService = true;

			var result = sut.Perform();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Once);
			sentinel.Verify(s => s.VerifyCursors(), Times.Once);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Once);
			logger.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Perform();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_VerifySessionIntegrity, key);
		}

		[TestMethod]
		public void Perform_MustDisableStickyKeysWhenNotAllowed()
		{
			nextSettings.Security.AllowStickyKeys = false;

			var result = sut.Perform();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustNotDisableStickyKeysWhenAllowed()
		{
			nextSettings.Security.AllowStickyKeys = true;

			var result = sut.Perform();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailWhenStickyKeysCannotBeDisabled()
		{
			nextSettings.Security.AllowStickyKeys = false;
			sentinel.Setup(s => s.DisableStickyKeys()).Returns(false);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustVerifyCursorsWhenEnabled()
		{
			nextSettings.Security.VerifyCursorConfiguration = true;

			sut.Perform();

			sentinel.Verify(s => s.VerifyCursors(), Times.Once);
		}

		[TestMethod]
		public void Perform_MustNotVerifyCursorsWhenDisabled()
		{
			nextSettings.Security.VerifyCursorConfiguration = false;
			sentinel.Setup(s => s.VerifyCursors()).Returns(false);

			var result = sut.Perform();

			sentinel.Verify(s => s.VerifyCursors(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailWhenCursorVerificationFails()
		{
			nextSettings.Security.VerifyCursorConfiguration = true;
			sentinel.Setup(s => s.VerifyCursors()).Returns(false);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);

		}

		[TestMethod]
		public void Perform_MustFailWhenEaseOfAccessIsCompromisedAndServiceIsIgnored()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Service.IgnoreService = true;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustTolerateCompromisedEaseOfAccessWhenServiceActiveInCurrentSession()
		{
			currentSettings.Service.IgnoreService = false;
			nextSettings.Service.IgnoreService = true;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustTolerateCompromisedEaseOfAccessWhenServiceActiveInNextSession()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Service.IgnoreService = false;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustTolerateCompromisedEaseOfAccessWithoutCurrentSessionWhenServiceActiveInNextSession()
		{
			runtimeContext.Current = null;
			nextSettings.Service.IgnoreService = false;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailWithoutCurrentSessionWhenEaseOfAccessIsCompromisedAndServiceIsIgnored()
		{
			runtimeContext.Current = null;
			nextSettings.Service.IgnoreService = true;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Perform_MustExecuteAllChecksEvenIfOneFails()
		{
			nextSettings.Security.AllowStickyKeys = false;
			nextSettings.Security.VerifyCursorConfiguration = true;
			sentinel.Setup(s => s.DisableStickyKeys()).Returns(false);

			var result = sut.Perform();

			sentinel.Verify(s => s.VerifyCursors(), Times.Once);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustSucceedWhenAllChecksSucceed()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Security.AllowStickyKeys = false;
			nextSettings.Security.VerifyCursorConfiguration = true;
			nextSettings.Service.IgnoreService = true;

			var result = sut.Repeat();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Once);
			sentinel.Verify(s => s.VerifyCursors(), Times.Once);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustFireStatusChangedEvent()
		{
			var fired = false;
			var key = default(TextKey);

			sut.StatusChanged += (k) =>
			{
				fired = true;
				key = k;
			};

			sut.Repeat();

			Assert.IsTrue(fired);
			Assert.AreEqual(TextKey.OperationStatus_VerifySessionIntegrity, key);
		}

		[TestMethod]
		public void Repeat_MustPerformSameChecksAsPerform()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Security.AllowStickyKeys = false;
			nextSettings.Security.VerifyCursorConfiguration = true;
			nextSettings.Service.IgnoreService = true;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);

			var result = sut.Repeat();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Once);
			sentinel.Verify(s => s.VerifyCursors(), Times.Once);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Once);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Repeat_MustNotDisableStickyKeysWhenAllowed()
		{
			nextSettings.Security.AllowStickyKeys = true;

			var result = sut.Repeat();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);
			sentinel.Verify(s => s.DisableStickyKeys(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustSucceedWhenCompromisedEaseOfAccessCanBeNeutralized()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Service.IgnoreService = true;
			sentinel.SetupSequence(s => s.VerifyEaseOfAccess()).Returns(false).Returns(true);
			sentinel.Setup(s => s.NeutralizeEaseOfAccess()).Returns(true);

			var result = sut.Perform();

			sentinel.Verify(s => s.NeutralizeEaseOfAccess(), Times.Once);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Exactly(2));
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustFailWhenCompromisedEaseOfAccessCannotBeNeutralized()
		{
			currentSettings.Service.IgnoreService = true;
			nextSettings.Service.IgnoreService = true;
			sentinel.Setup(s => s.VerifyEaseOfAccess()).Returns(false);
			sentinel.Setup(s => s.NeutralizeEaseOfAccess()).Returns(false);

			var result = sut.Perform();

			sentinel.Verify(s => s.NeutralizeEaseOfAccess(), Times.Once);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustRevertStickyKeys()
		{
			var result = sut.Revert();

			sentinel.Verify(s => s.RevertStickyKeys(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustRestoreEaseOfAccess()
		{
			var result = sut.Revert();

			sentinel.Verify(s => s.RestoreEaseOfAccess(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustNotPerformAnyVerification()
		{
			var result = sut.Revert();

			sentinel.Verify(s => s.DisableStickyKeys(), Times.Never);
			sentinel.Verify(s => s.VerifyCursors(), Times.Never);
			sentinel.Verify(s => s.VerifyEaseOfAccess(), Times.Never);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustSucceedEvenWhenStickyKeysCannotBeReverted()
		{
			sentinel.Setup(s => s.RevertStickyKeys()).Returns(false);

			var result = sut.Revert();

			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}