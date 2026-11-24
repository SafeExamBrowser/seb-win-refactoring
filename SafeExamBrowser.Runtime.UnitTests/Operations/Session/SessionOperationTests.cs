/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Operations.Session
{
	[TestClass]
	public class SessionOperationTests
	{
		private const TextKey MESSAGE_KEY = TextKey.MessageBox_ClientConfigurationError;
		private const TextKey TITLE_KEY = TextKey.MessageBox_ClientConfigurationErrorTitle;

		private ClientBridge clientBridge;
		private Mock<IClientProxy> clientProxy;
		private RuntimeContext context;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<IText> text;

		private TestableSessionOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			clientProxy = new Mock<IClientProxy>();
			context = new RuntimeContext();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			runtimeHost = new Mock<IRuntimeHost>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			text = new Mock<IText>();

			clientBridge = new ClientBridge(runtimeHost.Object, context);
			context.ClientProxy = clientProxy.Object;
			text.Setup(t => t.Get(It.IsAny<TextKey>())).Returns<TextKey>(key => key.ToString());

			var dependencies = new Dependencies(clientBridge, logger.Object, messageBox.Object, runtimeWindow.Object, context, text.Object);

			sut = new TestableSessionOperation(dependencies);
		}

		[TestMethod]
		public void MustCorrectlyInitializeDependencies()
		{
			Assert.AreSame(clientBridge, sut.ClientBridge);
			Assert.AreSame(context, sut.Context);
			Assert.AreSame(logger.Object, sut.Logger);
			Assert.AreSame(runtimeWindow.Object, sut.RuntimeWindow);
			Assert.AreSame(text.Object, sut.Text);
		}

		[TestMethod]
		public void ShowMessageBox_MustShowDirectlyDuringStartup()
		{
			context.Current = default;
			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Returns(MessageBoxResult.Cancel);

			var result = sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, MessageBoxAction.OkCancel, MessageBoxIcon.Warning);

			messageBox.Verify(m => m.Show(MESSAGE_KEY.ToString(), TITLE_KEY.ToString(), MessageBoxAction.OkCancel, MessageBoxIcon.Warning, runtimeWindow.Object), Times.Once);
			clientProxy.Verify(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

			Assert.AreEqual(MessageBoxResult.Cancel, result);
		}

		[TestMethod]
		public void ShowMessageBox_MustShowDirectlyWhenRunningOnDefaultDesktop()
		{
			context.Current = new SessionConfiguration { Settings = new AppSettings() };
			context.Current.Settings.Security.KioskMode = KioskMode.DisableExplorerShell;
			messageBox
				.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()))
				.Returns(MessageBoxResult.No);

			var result = sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, MessageBoxAction.YesNo, MessageBoxIcon.Question);

			messageBox.Verify(m => m.Show(MESSAGE_KEY.ToString(), TITLE_KEY.ToString(), MessageBoxAction.YesNo, MessageBoxIcon.Question, runtimeWindow.Object), Times.Once);
			clientProxy.Verify(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

			Assert.AreEqual(MessageBoxResult.No, result);
		}

		[TestMethod]
		public void ShowMessageBox_MustShowViaClientWhenRunningOnNewDesktop()
		{
			var replyReceived = new Action<string, string, int, int, Guid>((m, t, a, i, id) =>
			{
				runtimeHost.Raise(r => r.MessageBoxReplyReceived += null, new MessageBoxReplyEventArgs
				{
					RequestId = id,
					Result = (int) MessageBoxResult.Yes
				});
			});

			context.Current = new SessionConfiguration { Settings = new AppSettings() };
			context.Current.Settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
				.Callback(replyReceived)
				.Returns(new CommunicationResult(true));

			var result = sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, MessageBoxAction.YesNo, MessageBoxIcon.Question);

			clientProxy.Verify(c => c.ShowMessage(MESSAGE_KEY.ToString(), TITLE_KEY.ToString(), (int) MessageBoxAction.YesNo, (int) MessageBoxIcon.Question, It.IsAny<Guid>()), Times.Once);
			messageBox.VerifyNoOtherCalls();

			Assert.AreEqual(MessageBoxResult.Yes, result);
		}

		[TestMethod]
		public void ShowMessageBox_MustUseDefaultActionAndIcon()
		{
			sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY);

			messageBox.Verify(m => m.Show(MESSAGE_KEY.ToString(), TITLE_KEY.ToString(), MessageBoxAction.Ok, MessageBoxIcon.Information, runtimeWindow.Object), Times.Once);
		}

		[TestMethod]
		public void ShowMessageBox_MustReplaceMessagePlaceholders()
		{
			var placeholders = new Dictionary<string, string>
			{
				{ "%%URI%%", "https://www.safeexambrowser.org/config.seb" },
				{ "%%CODE%%", "42" }
			};

			text.Setup(t => t.Get(MESSAGE_KEY)).Returns("Failed to load %%URI%% with error %%CODE%% (%%URI%%).");

			sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, messagePlaceholders: placeholders);

			messageBox.Verify(m => m.Show(
				"Failed to load https://www.safeexambrowser.org/config.seb with error 42 (https://www.safeexambrowser.org/config.seb).",
				TITLE_KEY.ToString(),
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void ShowMessageBox_MustReplaceTitlePlaceholders()
		{
			var placeholders = new Dictionary<string, string>
			{
				{ "%%VERSION%%", "3.10.0" }
			};

			text.Setup(t => t.Get(TITLE_KEY)).Returns("Error in version %%VERSION%%");

			sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, titlePlaceholders: placeholders);

			messageBox.Verify(m => m.Show(
				MESSAGE_KEY.ToString(),
				"Error in version 3.10.0",
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void ShowMessageBox_MustNotModifyTextsWithoutPlaceholders()
		{
			text.Setup(t => t.Get(MESSAGE_KEY)).Returns("Message with %%PLACEHOLDER%%");
			text.Setup(t => t.Get(TITLE_KEY)).Returns("Title with %%PLACEHOLDER%%");

			sut.ShowMessageBox(MESSAGE_KEY, TITLE_KEY, messagePlaceholders: default, titlePlaceholders: new Dictionary<string, string>());

			messageBox.Verify(m => m.Show(
				"Message with %%PLACEHOLDER%%",
				"Title with %%PLACEHOLDER%%",
				It.IsAny<MessageBoxAction>(),
				It.IsAny<MessageBoxIcon>(),
				It.IsAny<IWindow>()), Times.Once);
		}

		[TestMethod]
		public void ShowMessageBox_MustForwardReplacedTextsToClient()
		{
			var replyReceived = new Action<string, string, int, int, Guid>((m, t, a, i, id) =>
			{
				runtimeHost.Raise(r => r.MessageBoxReplyReceived += null, new MessageBoxReplyEventArgs { RequestId = id, Result = (int) MessageBoxResult.Ok });
			});

			context.Current = new SessionConfiguration { Settings = new AppSettings() };
			context.Current.Settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			clientProxy
				.Setup(c => c.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
				.Callback(replyReceived)
				.Returns(new CommunicationResult(true));
			text.Setup(t => t.Get(MESSAGE_KEY)).Returns("Hello %%NAME%%");
			text.Setup(t => t.Get(TITLE_KEY)).Returns("%%TYPE%% Title");

			sut.ShowMessageBox(
				MESSAGE_KEY,
				TITLE_KEY,
				messagePlaceholders: new Dictionary<string, string> { { "%%NAME%%", "World" } },
				titlePlaceholders: new Dictionary<string, string> { { "%%TYPE%%", "Error" } });

			clientProxy.Verify(c => c.ShowMessage("Hello World", "Error Title", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
		}

		private class TestableSessionOperation : SessionOperation
		{
			public override event StatusChangedEventHandler StatusChanged;

			internal TestableSessionOperation(Dependencies dependencies) : base(dependencies)
			{
			}

			public new ClientBridge ClientBridge
			{
				get { return base.ClientBridge; }
			}

			public new RuntimeContext Context
			{
				get { return base.Context; }
			}

			public new ILogger Logger
			{
				get { return base.Logger; }
			}

			public new IRuntimeWindow RuntimeWindow
			{
				get { return base.RuntimeWindow; }
			}

			public new IText Text
			{
				get { return base.Text; }
			}

			public override OperationResult Perform()
			{
				StatusChanged?.Invoke(default);

				return OperationResult.Success;
			}

			public override OperationResult Repeat()
			{
				return OperationResult.Success;
			}

			public override OperationResult Revert()
			{
				return OperationResult.Success;
			}

			public new MessageBoxResult ShowMessageBox(
				TextKey messageKey,
				TextKey titleKey,
				MessageBoxAction action = MessageBoxAction.Ok,
				MessageBoxIcon icon = MessageBoxIcon.Information,
				IDictionary<string, string> messagePlaceholders = default,
				IDictionary<string, string> titlePlaceholders = default)
			{
				return base.ShowMessageBox(messageKey, titleKey, action, icon, messagePlaceholders, titlePlaceholders);
			}
		}
	}
}
