using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.UnitTests.Communication
{
	[TestClass]
	public class ClientBridgeTests
	{
		private Mock<IClientProxy> clientProxy;
		private RuntimeContext context;
		private Mock<IRuntimeHost> runtimeHost;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private ClientBridge sut;

		[TestInitialize]
		public void Initialize()
		{
			clientProxy = new Mock<IClientProxy>();
			context = new RuntimeContext();
			runtimeHost = new Mock<IRuntimeHost>();

			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			currentSession.Settings = currentSettings;

			context.ClientProxy = clientProxy.Object;
			context.Current = currentSession;

			sut = new ClientBridge(runtimeHost.Object, context);
		}

		[TestMethod]
		public void MustRequestServerExamSelectionCorrectly()
		{
			var exams = new[] { new Exam { Id = "abc1234" } };
			var examSelectionReceived = new Action<IEnumerable<(string, string, string, string)>, Guid>((e, id) =>
			{
				runtimeHost.Raise(r => r.ExamSelectionReceived += null, new ExamSelectionReplyEventArgs
				{
					RequestId = id,
					SelectedExamId = "abc1234",
					Success = true
				});
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.RequestExamSelection(It.IsAny<IEnumerable<(string, string, string, string)>>(), It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(examSelectionReceived);

			sut.TryAskForExamSelection(exams, out var exam);

			clientProxy.Verify(c => c.RequestExamSelection(It.IsAny<IEnumerable<(string, string, string, string)>>(), It.IsAny<Guid>()), Times.Once);

			Assert.IsTrue(sut.IsRequired());
			Assert.AreEqual("abc1234", exam.Id);
		}

		[TestMethod]
		public void MustRequestPasswordCorrectly()
		{
			var passwordReceived = new Action<PasswordRequestPurpose, Guid>((p, id) =>
			{
				runtimeHost.Raise(r => r.PasswordReceived += null, new PasswordReplyEventArgs { Password = "test", RequestId = id, Success = true });
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(passwordReceived);

			sut.TryGetPassword(default, out var password);

			clientProxy.Verify(c => c.RequestPassword(It.IsAny<PasswordRequestPurpose>(), It.IsAny<Guid>()), Times.Once);

			Assert.IsTrue(sut.IsRequired());
			Assert.AreEqual("test", password);
		}

		[TestMethod]
		public void MustRequestServerFailureActionCorrectly()
		{
			var failureActionReceived = new Action<string, bool, Guid>((m, f, id) =>
			{
				runtimeHost.Raise(r => r.ServerFailureActionReceived += null, new ServerFailureActionReplyEventArgs
				{
					RequestId = id,
					Fallback = true
				});
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.RequestServerFailureAction(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>()))
				.Returns(new CommunicationResult(true))
				.Callback(failureActionReceived);

			sut.TryAskForServerFailureAction(default, default, out var abort, out var fallback, out var retry);

			clientProxy.Verify(c => c.RequestServerFailureAction(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>()), Times.Once);

			Assert.IsTrue(sut.IsRequired());
			Assert.IsFalse(abort);
			Assert.IsTrue(fallback);
			Assert.IsFalse(retry);
		}

		[TestMethod]
		public void MustShowMessageBoxCorrectly()
		{
			var replyReceived = new Action<string, string, int, int, Guid>((m, t, a, i, id) =>
			{
				runtimeHost.Raise(r => r.MessageBoxReplyReceived += null, new MessageBoxReplyEventArgs
				{
					RequestId = id,
					Result = (int) MessageBoxResult.Yes
				});
			});

			currentSettings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
				.Callback(replyReceived)
				.Returns(new CommunicationResult(true));

			var result = sut.ShowMessageBox(default, default, default, default);

			clientProxy.Verify(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);

			Assert.IsTrue(sut.IsRequired());
			Assert.AreEqual(MessageBoxResult.Yes, result);
		}
	}
}
