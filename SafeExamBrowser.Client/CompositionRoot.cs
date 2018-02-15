/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Client.Behaviour;
using SafeExamBrowser.Client.Behaviour.Operations;
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.Communication;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.Keyboard;
using SafeExamBrowser.Monitoring.Mouse;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.Monitoring.Windows;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private ClientConfiguration configuration;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private ISystemInfo systemInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		internal IClientController ClientController { get; private set; }
		internal Taskbar Taskbar { get; private set; }

		internal void BuildObjectGraph()
		{
			var args = Environment.GetCommandLineArgs();

			Validate(args);

			configuration = new ClientConfiguration();
			logger = new Logger();
			nativeMethods = new NativeMethods();
			systemInfo = new SystemInfo();

			InitializeLogging(args[1]);

			text = new Text(logger);
			uiFactory = new UserInterfaceFactory(text);

			var runtimeProxy = new RuntimeProxy(args[2], new ModuleLogger(logger, typeof(RuntimeProxy)));
			var displayMonitor = new DisplayMonitor(new ModuleLogger(logger, typeof(DisplayMonitor)), nativeMethods);
			var processMonitor = new ProcessMonitor(new ModuleLogger(logger, typeof(ProcessMonitor)), nativeMethods);
			var windowMonitor = new WindowMonitor(new ModuleLogger(logger, typeof(WindowMonitor)), nativeMethods);

			Taskbar = new Taskbar(new ModuleLogger(logger, typeof(Taskbar)));

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text));
			operations.Enqueue(new RuntimeConnectionOperation(logger, runtimeProxy, Guid.Parse(args[3])));
			operations.Enqueue(new ConfigurationOperation(configuration, logger, runtimeProxy));
			operations.Enqueue(new DelayedInitializationOperation(BuildCommunicationHostOperation));
			operations.Enqueue(new DelayedInitializationOperation(BuildKeyboardInterceptorOperation));
			operations.Enqueue(new WindowMonitorOperation(logger, windowMonitor));
			operations.Enqueue(new ProcessMonitorOperation(logger, processMonitor));
			operations.Enqueue(new DisplayMonitorOperation(displayMonitor, logger, Taskbar));
			operations.Enqueue(new DelayedInitializationOperation(BuildTaskbarOperation));
			operations.Enqueue(new DelayedInitializationOperation(BuildBrowserOperation));
			operations.Enqueue(new ClipboardOperation(logger, nativeMethods));
			operations.Enqueue(new DelayedInitializationOperation(BuildMouseInterceptorOperation));

			var sequence = new OperationSequence(logger, operations);

			ClientController = new ClientController(displayMonitor, logger, sequence, processMonitor, runtimeProxy, Taskbar, windowMonitor);
		}

		private void Validate(string[] args)
		{
			var hasFour = args?.Length == 4;

			if (hasFour)
			{
				var hasLogfilePath = Uri.TryCreate(args?[1], UriKind.Absolute, out Uri filePath) && filePath.IsFile;
				var hasHostUri = Uri.TryCreate(args?[2], UriKind.Absolute, out Uri hostUri) && hostUri.IsWellFormedOriginalString();
				var hasToken = Guid.TryParse(args?[3], out Guid token);

				if (hasLogfilePath && hasHostUri && hasToken)
				{
					return;
				}
			}

			throw new ArgumentException("Invalid parameters! Required: SafeExamBrowser.Client.exe <logfile path> <host URI> <token>");
		}

		private void InitializeLogging(string filePath)
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), filePath);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}

		private IOperation BuildBrowserOperation()
		{
			var browserController = new BrowserApplicationController(configuration.Settings.Browser, configuration.RuntimeInfo, text, uiFactory);
			var browserInfo = new BrowserApplicationInfo();
			var operation = new BrowserOperation(browserController, browserInfo, logger, Taskbar, uiFactory);

			return operation;
		}

		private IOperation BuildCommunicationHostOperation()
		{
			var host = new ClientHost(configuration.RuntimeInfo.ClientAddress, new ModuleLogger(logger, typeof(ClientHost)));
			var operation = new CommunicationOperation(host, logger);

			return operation;
		}

		private IOperation BuildKeyboardInterceptorOperation()
		{
			var keyboardInterceptor = new KeyboardInterceptor(configuration.Settings.Keyboard, new ModuleLogger(logger, typeof(KeyboardInterceptor)));
			var operation = new KeyboardInterceptorOperation(keyboardInterceptor, logger, nativeMethods);

			return operation;
		}

		private IOperation BuildMouseInterceptorOperation()
		{
			var mouseInterceptor = new MouseInterceptor(new ModuleLogger(logger, typeof(MouseInterceptor)), configuration.Settings.Mouse);
			var operation = new MouseInterceptorOperation(logger, mouseInterceptor, nativeMethods);

			return operation;
		}

		private IOperation BuildTaskbarOperation()
		{
			var keyboardLayout = new KeyboardLayout(new ModuleLogger(logger, typeof(KeyboardLayout)), text);
			var powerSupply = new PowerSupply(new ModuleLogger(logger, typeof(PowerSupply)), text);
			var wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, typeof(WirelessNetwork)), text);
			var operation = new TaskbarOperation(logger, configuration.Settings.Taskbar, keyboardLayout, powerSupply, wirelessNetwork, systemInfo, Taskbar, text, uiFactory);

			return operation;
		}
	}
}
