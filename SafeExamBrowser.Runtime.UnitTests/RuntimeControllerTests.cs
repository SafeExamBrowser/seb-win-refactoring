/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeExamBrowser.Runtime.UnitTests
{
	[TestClass]
	public class RuntimeControllerTests
	{
		[TestMethod]
		public void TODO()
		{
			Assert.Fail();
		}

		//[TestMethod]
		//public void MustRequestPasswordViaDialogOnDefaultDesktop()
		//{
		//	var clientProxy = new Mock<IClientProxy>();
		//	var session = new Mock<ISessionData>();
		//	var url = @"http://www.safeexambrowser.org/whatever.seb";

		//	passwordDialog.Setup(d => d.Show(null)).Returns(new PasswordDialogResultStub { Success = true });
		//	repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
		//	repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
		//	session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
		//	settings.KioskMode = KioskMode.DisableExplorerShell;

		//	sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
		//	sut.Perform();

		//	clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Never);
		//	passwordDialog.Verify(p => p.Show(null), Times.AtLeastOnce);
		//	session.VerifyGet(s => s.ClientProxy, Times.Never);
		//}

		//[TestMethod]
		//public void MustRequestPasswordViaClientDuringReconfigurationOnNewDesktop()
		//{
		//	var clientProxy = new Mock<IClientProxy>();
		//	var communication = new CommunicationResult(true);
		//	var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
		//	{
		//		runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = true });
		//	});
		//	var session = new Mock<ISessionData>();
		//	var url = @"http://www.safeexambrowser.org/whatever.seb";

		//	clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication).Callback(passwordReceived);
		//	passwordDialog.Setup(d => d.Show(null)).Returns(new PasswordDialogResultStub { Success = true });
		//	repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
		//	repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
		//	session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
		//	settings.KioskMode = KioskMode.CreateNewDesktop;

		//	sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });
		//	sut.Perform();

		//	clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.AtLeastOnce);
		//	passwordDialog.Verify(p => p.Show(null), Times.Never);
		//	session.VerifyGet(s => s.ClientProxy, Times.AtLeastOnce);
		//}

		//[TestMethod]
		//public void MustAbortAskingForPasswordViaClientIfDecidedByUser()
		//{
		//	var clientProxy = new Mock<IClientProxy>();
		//	var communication = new CommunicationResult(true);
		//	var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
		//	{
		//		runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { RequestId = id, Success = false });
		//	});
		//	var session = new Mock<ISessionData>();
		//	var url = @"http://www.safeexambrowser.org/whatever.seb";

		//	clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication).Callback(passwordReceived);
		//	repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
		//	repository.Setup(r => r.LoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
		//	session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
		//	settings.KioskMode = KioskMode.CreateNewDesktop;

		//	sut = new ConfigurationOperation(appConfig, repository.Object, logger.Object, resourceLoader.Object, new[] { "blubb.exe", url });

		//	var result = sut.Perform();

		//	Assert.AreEqual(OperationResult.Aborted, result);
		//}

		//[TestMethod]
		//public void MustNotWaitForPasswordViaClientIfCommunicationHasFailed()
		//{
		//	var clientProxy = new Mock<IClientProxy>();
		//	var communication = new CommunicationResult(false);
		//	var session = new Mock<ISessionData>();
		//	var url = @"http://www.safeexambrowser.org/whatever.seb";

		//	clientProxy.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>())).Returns(communication);
		//	repository.SetupGet(r => r.CurrentSession).Returns(session.Object);
		//	repository.Setup(r => r.TryLoadSettings(It.IsAny<Uri>(), null, null)).Returns(LoadStatus.SettingsPasswordNeeded);
		//	session.SetupGet(r => r.ClientProxy).Returns(clientProxy.Object);
		//	settings.KioskMode = KioskMode.CreateNewDesktop;

		//	sut = new ConfigurationOperation(new[] { "blubb.exe", url }, repository.Object, logger.Object, resourceLoader.Object, sessionContext);

		//	var result = sut.Perform();

		//	Assert.AreEqual(OperationResult.Aborted, result);
		//}
	}
}
