using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Responsibilities
{
	[TestClass]
	public class SessionResponsibilityTests
	{
		private AppConfig appConfig;
		private Mock<IClientProxy> clientProxy;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IResponsibilityCollection<RuntimeTask>> responsibilities;
		private RuntimeContext context;
		private Mock<IRuntimeWindow> runtimeWindow;
		private Mock<IRepeatableOperationSequence> sessionSequence;
		private Mock<Action> shutdown;
		private Mock<IText> text;
		private SessionConfiguration currentSession;
		private AppSettings currentSettings;
		private SessionConfiguration nextSession;
		private AppSettings nextSettings;
		private SessionResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			clientProxy = new Mock<IClientProxy>();
			context = new RuntimeContext();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			responsibilities = new Mock<IResponsibilityCollection<RuntimeTask>>();
			runtimeWindow = new Mock<IRuntimeWindow>();
			sessionSequence = new Mock<IRepeatableOperationSequence>();
			shutdown = new Mock<Action>();
			text = new Mock<IText>();

			currentSession = new SessionConfiguration();
			currentSettings = new AppSettings();
			nextSession = new SessionConfiguration();
			nextSettings = new AppSettings();

			currentSession.Settings = currentSettings;
			nextSession.Settings = nextSettings;

			context.ClientProxy = clientProxy.Object;
			context.Current = currentSession;
			context.Next = nextSession;
			context.Responsibilities = responsibilities.Object;

			sut = new SessionResponsibility(appConfig, logger.Object, messageBox.Object, context, runtimeWindow.Object, sessionSequence.Object, shutdown.Object, text.Object);
		}

		[TestMethod]
		public void MustHideRuntimeWindowWhenUsingDisableExplorerShell()
		{
			context.Current = default;
			currentSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			nextSettings.Security.KioskMode = KioskMode.DisableExplorerShell;
			sessionSequence.Setup(s => s.TryPerform()).Callback(() => context.Current = currentSession).Returns(OperationResult.Success);

			sut.Assume(RuntimeTask.StartSession);

			runtimeWindow.Verify(w => w.Hide(), Times.Once);
			runtimeWindow.Reset();
			sessionSequence.Reset();
			sessionSequence.Setup(s => s.TryRepeat()).Returns(OperationResult.Aborted);

			sut.Assume(RuntimeTask.StartSession);

			runtimeWindow.Verify(w => w.Hide(), Times.Once);
		}

		[TestMethod]
		public void MustShowMessageBoxOnFailure()
		{
			context.Current = default;
			sessionSequence.Setup(b => b.TryPerform()).Callback(() => context.Current = currentSession).Returns(OperationResult.Failed);

			sut.Assume(RuntimeTask.StartSession);

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.AtLeastOnce);
			messageBox.Reset();
			sessionSequence.Reset();
			sessionSequence.Setup(b => b.TryRepeat()).Returns(OperationResult.Failed);

			sut.Assume(RuntimeTask.StartSession);

			messageBox.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxAction>(), It.IsAny<MessageBoxIcon>(), It.IsAny<IWindow>()), Times.AtLeastOnce);
		}

		[TestMethod]
		public void MustTerminateOnSessionStartFailure()
		{
			context.Current = default;
			sessionSequence.Setup(b => b.TryPerform()).Callback(() => context.Current = currentSession).Returns(OperationResult.Failed);

			sut.Assume(RuntimeTask.StartSession);

			sessionSequence.Verify(b => b.TryPerform(), Times.Once);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Once);
			shutdown.Verify(s => s(), Times.Once);
		}

		[TestMethod]
		public void MustNotTerminateOnSessionStartAbortion()
		{
			context.Current = default;
			sessionSequence.Setup(b => b.TryPerform()).Returns(OperationResult.Aborted);

			sut.Assume(RuntimeTask.StartSession);

			sessionSequence.Verify(b => b.TryPerform(), Times.Once);
			sessionSequence.Verify(b => b.TryRepeat(), Times.Never);
			sessionSequence.Verify(b => b.TryRevert(), Times.Never);
			shutdown.Verify(s => s(), Times.Never);
		}

		[TestMethod]
		public void MustInformClientAboutAbortedReconfiguration()
		{
			context.Current = default;
			sessionSequence.Setup(b => b.TryPerform()).Callback(() => context.Current = currentSession).Returns(OperationResult.Aborted);

			sut.Assume(RuntimeTask.StartSession);

			clientProxy.Verify(c => c.InformReconfigurationAborted(), Times.Once);
		}
	}
}
