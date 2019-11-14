/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Applications;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Notifications;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Applications;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.Keyboard;
using SafeExamBrowser.Monitoring.Mouse;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.UserInterface;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.SystemComponents.Audio;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Keyboard;
using SafeExamBrowser.SystemComponents.PowerSupply;
using SafeExamBrowser.SystemComponents.WirelessNetwork;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Activators;
using SafeExamBrowser.WindowsApi;
using SafeExamBrowser.WindowsApi.Contracts;
using Desktop = SafeExamBrowser.UserInterface.Desktop;
using Mobile = SafeExamBrowser.UserInterface.Mobile;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private const int TWO_SECONDS = 2000;
		private const int FIVE_SECONDS = 5000;

		private Guid authenticationToken;
		private ClientContext context;
		private string logFilePath;
		private LogLevel logLevel;
		private string runtimeHostUri;
		private UserInterfaceMode uiMode;

		private IActionCenter actionCenter;
		private ILogger logger;
		private IMessageBox messageBox;
		private INativeMethods nativeMethods;
		private IRuntimeProxy runtimeProxy;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private IText text;
		private ITextResource textResource;
		private IUserInterfaceFactory uiFactory;

		internal IClientController ClientController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			ValidateCommandLineArguments();

			logger = new Logger();
			nativeMethods = new NativeMethods();
			systemInfo = new SystemInfo();

			InitializeLogging();
			InitializeText();

			actionCenter = BuildActionCenter();
			context = new ClientContext();
			messageBox = BuildMessageBox();
			uiFactory = BuildUserInterfaceFactory();
			runtimeProxy = new RuntimeProxy(runtimeHostUri, new ProxyObjectFactory(), ModuleLogger(nameof(RuntimeProxy)), Interlocutor.Client);
			taskbar = BuildTaskbar();

			var processFactory = new ProcessFactory(ModuleLogger(nameof(ProcessFactory)));
			var applicationFactory = new ApplicationFactory(ModuleLogger(nameof(ApplicationFactory)), processFactory);
			var applicationMonitor = new ApplicationMonitor(TWO_SECONDS, ModuleLogger(nameof(ApplicationMonitor)), nativeMethods, new ProcessFactory(ModuleLogger(nameof(ProcessFactory))));
			var displayMonitor = new DisplayMonitor(ModuleLogger(nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(ModuleLogger(nameof(ExplorerShell)), nativeMethods);
			var hashAlgorithm = new HashAlgorithm();

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text, textResource));
			operations.Enqueue(new RuntimeConnectionOperation(context, logger, runtimeProxy, authenticationToken));
			operations.Enqueue(new ConfigurationOperation(context, logger, runtimeProxy));
			operations.Enqueue(new DelegateOperation(UpdateAppConfig));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostOperation));
			operations.Enqueue(new ClientHostDisconnectionOperation(context, logger, FIVE_SECONDS));
			operations.Enqueue(new LazyInitializationOperation(BuildKeyboardInterceptorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildMouseInterceptorOperation));
			operations.Enqueue(new ApplicationOperation(context, applicationFactory, applicationMonitor, logger, text));
			operations.Enqueue(new DisplayMonitorOperation(context, displayMonitor, logger, taskbar));
			operations.Enqueue(new LazyInitializationOperation(BuildShellOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildBrowserOperation));
			operations.Enqueue(new ClipboardOperation(context, logger, nativeMethods));

			var sequence = new OperationSequence(logger, operations);

			ClientController = new ClientController(
				actionCenter,
				applicationMonitor,
				context,
				displayMonitor,
				explorerShell,
				hashAlgorithm,
				logger,
				messageBox,
				sequence,
				runtimeProxy,
				shutdown,
				taskbar,
				text,
				uiFactory);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# New client instance started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Client instance terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void ValidateCommandLineArguments()
		{
			var args = Environment.GetCommandLineArgs();
			var hasFive = args?.Length >= 5;

			if (hasFive)
			{
				var hasLogfilePath = Uri.TryCreate(args[1], UriKind.Absolute, out Uri filePath) && filePath.IsFile;
				var hasLogLevel = Enum.TryParse(args[2], out LogLevel level);
				var hasHostUri = Uri.TryCreate(args[3], UriKind.Absolute, out Uri hostUri) && hostUri.IsWellFormedOriginalString();
				var hasAuthenticationToken = Guid.TryParse(args[4], out Guid token);

				if (hasLogfilePath && hasLogLevel && hasHostUri && hasAuthenticationToken)
				{
					logFilePath = args[1];
					logLevel = level;
					runtimeHostUri = args[3];
					authenticationToken = token;
					uiMode = args.Length == 6 && Enum.TryParse(args[5], out uiMode) ? uiMode : UserInterfaceMode.Desktop;

					return;
				}
			}

			throw new ArgumentException("Invalid arguments! Required: SafeExamBrowser.Client.exe <logfile path> <log level> <host URI> <token>");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

			logFileWriter.Initialize();
			logger.LogLevel = logLevel;
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text.xml";

			text = new Text(logger);
			textResource = new XmlTextResource(path);
		}

		private IOperation BuildBrowserOperation()
		{
			var moduleLogger = ModuleLogger(nameof(BrowserApplication));
			var browser = new BrowserApplication(context.AppConfig, context.Settings.Browser, messageBox, moduleLogger, text, uiFactory);
			var operation = new BrowserOperation(actionCenter, context, logger, taskbar, uiFactory);

			context.Browser = browser;

			return operation;
		}

		private IOperation BuildClientHostOperation()
		{
			var processId = Process.GetCurrentProcess().Id;
			var factory = new HostObjectFactory();
			var clientHost = new ClientHost(context.AppConfig.ClientAddress, factory, ModuleLogger(nameof(ClientHost)), processId, FIVE_SECONDS);
			var operation = new CommunicationHostOperation(clientHost, logger);

			context.ClientHost = clientHost;
			context.ClientHost.AuthenticationToken = authenticationToken;

			return operation;
		}

		private IOperation BuildKeyboardInterceptorOperation()
		{
			var keyboardInterceptor = new KeyboardInterceptor(ModuleLogger(nameof(KeyboardInterceptor)), nativeMethods, context.Settings.Keyboard);
			var operation = new KeyboardInterceptorOperation(context, keyboardInterceptor, logger);

			return operation;
		}

		private IOperation BuildMouseInterceptorOperation()
		{
			var mouseInterceptor = new MouseInterceptor(ModuleLogger(nameof(MouseInterceptor)), nativeMethods, context.Settings.Mouse);
			var operation = new MouseInterceptorOperation(context, logger, mouseInterceptor);

			return operation;
		}

		private IOperation BuildShellOperation()
		{
			var aboutInfo = new AboutNotificationInfo(text);
			var aboutController = new AboutNotificationController(context.AppConfig, uiFactory);
			var audio = new Audio(context.Settings.Audio, ModuleLogger(nameof(Audio)));
			var keyboard = new Keyboard(ModuleLogger(nameof(Keyboard)));
			var logInfo = new LogNotificationInfo(text);
			var logController = new LogNotificationController(logger, uiFactory);
			var powerSupply = new PowerSupply(ModuleLogger(nameof(PowerSupply)));
			var taskView = BuildTaskView();
			var wirelessAdapter = new WirelessAdapter(ModuleLogger(nameof(WirelessAdapter)));
			var operation = new ShellOperation(
				actionCenter,
				audio,
				aboutInfo,
				aboutController,
				context,
				keyboard,
				logger,
				logInfo,
				logController,
				powerSupply,
				systemInfo,
				taskbar,
				taskView,
				text,
				uiFactory,
				wirelessAdapter);

			context.Activators.Add(new ActionCenterKeyboardActivator(ModuleLogger(nameof(ActionCenterKeyboardActivator)), nativeMethods));
			context.Activators.Add(new ActionCenterTouchActivator(ModuleLogger(nameof(ActionCenterTouchActivator)), nativeMethods));
			context.Activators.Add(new TaskViewKeyboardActivator(ModuleLogger(nameof(TaskViewKeyboardActivator)), nativeMethods));
			context.Activators.Add(new TerminationActivator(ModuleLogger(nameof(TerminationActivator)), nativeMethods));

			return operation;
		}

		private IActionCenter BuildActionCenter()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.ActionCenter();
				default:
					return new Desktop.ActionCenter();
			}
		}

		private IMessageBox BuildMessageBox()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.MessageBox(text);
				default:
					return new Desktop.MessageBox(text);
			}
		}

		private ITaskbar BuildTaskbar()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.Taskbar(ModuleLogger(nameof(Mobile.Taskbar)));
				default:
					return new Desktop.Taskbar(ModuleLogger(nameof(Desktop.Taskbar)));
			}
		}

		private ITaskView BuildTaskView()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					// TODO
					throw new NotImplementedException();
				default:
					return new Desktop.TaskView();
			}
		}

		private IUserInterfaceFactory BuildUserInterfaceFactory()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.UserInterfaceFactory(text);
				default:
					return new Desktop.UserInterfaceFactory(text);
			}
		}

		private void UpdateAppConfig()
		{
			ClientController.UpdateAppConfig();
		}

		private IModuleLogger ModuleLogger(string moduleInfo)
		{
			return new ModuleLogger(logger, moduleInfo);
		}
	}
}
