/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using CefSharp;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Responsibilities;
using SafeExamBrowser.Browser.Responsibilities.Browser;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Core.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.WindowsApi.Contracts;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplication : IBrowserApplication
	{
		private readonly BrowserApplicationContext context;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly IKeyGenerator keyGenerator;
		private readonly IModuleLogger logger;
		private readonly IMessageBox messageBox;
		private readonly INativeMethods nativeMethods;
		private readonly SessionMode sessionMode;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private IResponsibilityCollection<BrowserTask> Responsibilities => context.Responsibilities;
		private IList<BrowserWindow> Windows => context.Windows;

		public bool AutoStart { get; private set; }
		public IconResource Icon { get; private set; }
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Tooltip { get; private set; }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event LoseFocusRequestedEventHandler LoseFocusRequested;
		public event TerminationRequestedEventHandler TerminationRequested;
		public event UserIdentifierDetectedEventHandler UserIdentifierDetected;
		public event WindowsChangedEventHandler WindowsChanged;

		public BrowserApplication(
			AppConfig appConfig,
			BrowserSettings settings,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			IKeyGenerator keyGenerator,
			IMessageBox messageBox,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			SessionMode sessionMode,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.keyGenerator = keyGenerator;
			this.logger = logger;
			this.messageBox = messageBox;
			this.nativeMethods = nativeMethods;
			this.sessionMode = sessionMode;
			this.text = text;
			this.uiFactory = uiFactory;

			context = new BrowserApplicationContext
			{
				AppConfig = appConfig,
				Logger = logger,
				Settings = settings
			};
		}

		public void Focus(bool forward)
		{
			foreach (var window in Windows)
			{
				window.Focus(forward);
			}
		}

		public IEnumerable<IBrowserWindow> GetWindows()
		{
			return new List<IBrowserWindow>(Windows);
		}

		public void Initialize()
		{
			logger.Info("Starting initialization...");

			InitializeResponsibilities();

			Responsibilities.Delegate(BrowserTask.InitializeBrowserConfiguration);

			var success = Cef.Initialize(context.CefSettings, true, default(IApp));

			InitializeApplicationInfo();

			if (success)
			{
				Responsibilities.Delegate(BrowserTask.InitializeCookies);
				Responsibilities.Delegate(BrowserTask.InitializeFileSystem);
				Responsibilities.Delegate(BrowserTask.InitializeIntegrity);
				Responsibilities.Delegate(BrowserTask.InitializePreferences);

				logger.Info("Initialized browser.");
			}
			else
			{
				throw new Exception($"Failed to initialize browser (exit code: {Cef.GetExitCode()})!");
			}
		}

		public void Start()
		{
			Responsibilities.Delegate(BrowserTask.CreateMainWindow);
		}

		public void Terminate()
		{
			logger.Info("Initiating termination...");

			AwaitReady();

			Responsibilities.Delegate(BrowserTask.CloseAllWindows);
			Responsibilities.Delegate(BrowserTask.FinalizeCookies);
			Responsibilities.Delegate(BrowserTask.FinalizeFileSystem);

			Cef.Shutdown();

			Responsibilities.Delegate(BrowserTask.FinalizeCache);

			logger.Info("Terminated browser.");
		}

		internal static void AwaitReady()
		{
			// We apparently need to let the browser finish any pending work before attempting to reset or terminate it, especially if the
			// reset or termination is initiated automatically (e.g. by a quit URL). Otherwise, the engine will crash on some occasions, seemingly
			// when it can't finish handling its events (like ChromiumWebBrowser.LoadError).

			Thread.Sleep(500);
		}

		private void InitializeApplicationInfo()
		{
			AutoStart = true;
			Icon = new BrowserIconResource();
			Id = Guid.NewGuid();
			Name = text.Get(TextKey.Browser_Name);
			Tooltip = text.Get(TextKey.Browser_Tooltip);
		}

		private void InitializeResponsibilities()
		{
			var windowHandlingResponsibility = new WindowHandlingResponsibility(context, fileSystemDialog, hashAlgorithm, keyGenerator, messageBox, nativeMethods, sessionMode, text, uiFactory);

			windowHandlingResponsibility.ConfigurationDownloadRequested += (f, a) => ConfigurationDownloadRequested?.Invoke(f, a);
			windowHandlingResponsibility.LoseFocusRequested += (f) => LoseFocusRequested?.Invoke(f);
			windowHandlingResponsibility.TerminationRequested += () => TerminationRequested?.Invoke();
			windowHandlingResponsibility.UserIdentifierDetected += (i) => UserIdentifierDetected?.Invoke(i);
			windowHandlingResponsibility.WindowsChanged += () => WindowsChanged.Invoke();

			context.Responsibilities = new ResponsibilityCollection<BrowserTask>(logger, new BrowserResponsibility[]
			{
				new CacheResponsibility(context),
				new ConfigurationResponsibility(context),
				new FileSystemResponsibility(context),
				new IntegrityResponsibility(context, keyGenerator),
				windowHandlingResponsibility
			});
		}
	}
}
