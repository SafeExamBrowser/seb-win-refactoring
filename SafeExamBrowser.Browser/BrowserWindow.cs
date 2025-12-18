/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Browser.Integrations;
using SafeExamBrowser.Browser.Responsibilities;
using SafeExamBrowser.Browser.Responsibilities.Window;
using SafeExamBrowser.Browser.Wrapper;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Core.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using RequestHandler = SafeExamBrowser.Browser.Handlers.RequestHandler;
using ResourceHandler = SafeExamBrowser.Browser.Handlers.ResourceHandler;
using TitleChangedEventHandler = SafeExamBrowser.Applications.Contracts.Events.TitleChangedEventHandler;

namespace SafeExamBrowser.Browser
{
	internal class BrowserWindow : Contracts.IBrowserWindow
	{
		private readonly AppConfig appConfig;
		private readonly BrowserWindowContext context;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IKeyGenerator keyGenerator;
		private readonly SessionMode sessionMode;
		private readonly IModuleLogger logger;

		private IEnumerable<Integration> integrations;
		private IResponsibilityCollection<WindowTask> responsibilities;

		private BrowserSettings Settings => context.Settings;
		private IBrowserWindow Window => context.Window;

		internal IBrowserControl Control => context.Control;
		internal int Id => context.Id;

		public IntPtr Handle { get; private set; }
		public IconResource Icon => context.Icon;
		public bool IsMainWindow => context.IsMainWindow;
		public string Title => context.Title;
		public string Url => context.Url;

		internal event WindowClosedEventHandler Closed;
		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event LoseFocusRequestedEventHandler LoseFocusRequested;
		internal event PopupRequestedEventHandler PopupRequested;
		internal event ResetRequestedEventHandler ResetRequested;
		internal event TerminationRequestedEventHandler TerminationRequested;
		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

		public event IconChangedEventHandler IconChanged;
		public event TitleChangedEventHandler TitleChanged;

		public BrowserWindow(
			AppConfig appConfig,
			BrowserWindowContext context,
			IFileSystemDialog fileSystemDialog,
			IKeyGenerator keyGenerator,
			SessionMode sessionMode)
		{
			this.appConfig = appConfig;
			this.context = context;
			this.fileSystemDialog = fileSystemDialog;
			this.keyGenerator = keyGenerator;
			this.logger = context.Logger;
			this.sessionMode = sessionMode;
		}

		public void Activate()
		{
			Window.BringToForeground();
		}

		internal void Close()
		{
			Window.Close();
			Control.Destroy();
		}

		internal void Focus(bool forward)
		{
			if (forward)
			{
				Window.FocusToolbar(forward);
			}
			else
			{
				Window.FocusBrowser();
				Activate();
			}
		}

		internal void Initialize()
		{
			var windowSettings = IsMainWindow ? Settings.MainWindow : Settings.AdditionalWindow;

			integrations = new Integration[]
			{
				new GenericIntegration(logger.CloneFor($"{nameof(GenericIntegration)} #{Id}")),
				new EdxIntegration(logger.CloneFor($"{nameof(EdxIntegration)} #{Id}")),
				new MoodleIntegration(logger.CloneFor($"{nameof(MoodleIntegration)} #{Id}"))
			};

			var cefSharpControl = default(ICefSharpControl);
			var clipboard = new Clipboard(logger.CloneFor(nameof(Clipboard)), Settings);
			var controlLogger = logger.CloneFor($"{nameof(BrowserControl)} #{Id}");
			var contextMenuHandler = new ContextMenuHandler();
			var dialogHandler = new DialogHandler();
			var displayHandler = new DisplayHandler();
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} #{Id}");
			var downloadHandler = new DownloadHandler(appConfig, downloadLogger, Settings, windowSettings);
			var dragHandler = new DragHandler();
			var focusHandler = new FocusHandler();
			var javaScriptDialogHandler = new JavaScriptDialogHandler();
			var keyboardHandler = new KeyboardHandler();
			var renderHandler = new RenderProcessMessageHandler(appConfig, clipboard, keyGenerator, Settings, context.Text);
			var requestFilter = new RequestFilter();
			var requestLogger = logger.CloneFor($"{nameof(RequestHandler)} #{Id}");
			var resourceHandler = new ResourceHandler(appConfig, requestFilter, integrations, keyGenerator, logger, sessionMode, Settings, windowSettings, context.Text);
			var requestHandler = new RequestHandler(appConfig, requestFilter, requestLogger, resourceHandler, Settings, windowSettings);

			InitializeResponsibilities(
				dialogHandler,
				displayHandler,
				downloadHandler,
				fileSystemDialog,
				javaScriptDialogHandler,
				keyboardHandler,
				requestFilter,
				requestHandler,
				resourceHandler);

			if (IsMainWindow)
			{
				responsibilities.Delegate(WindowTask.InitializeLifeSpanHandler);
				cefSharpControl = new CefSharpBrowserControl(context.LifeSpanHandler, context.StartUrl);
			}
			else
			{
				cefSharpControl = new CefSharpPopupControl();
			}

			responsibilities.Delegate(WindowTask.InitializeRequestFilter);

			context.Control = new BrowserControl(
				clipboard,
				cefSharpControl,
				contextMenuHandler,
				dialogHandler,
				displayHandler,
				downloadHandler,
				dragHandler,
				focusHandler,
				javaScriptDialogHandler,
				keyboardHandler,
				controlLogger,
				renderHandler,
				requestHandler);

			Control.Initialize();

			logger.Debug("Initialized browser control.");
		}

		internal void Show()
		{
			context.Window = context.UserInterfaceFactory.CreateBrowserWindow(Control, context.Settings, IsMainWindow, logger);

			responsibilities.Delegate(WindowTask.RegisterEvents);
			responsibilities.Delegate(WindowTask.InitiateCookieTraversal);
			responsibilities.Delegate(WindowTask.InitializeZoom);

			Window.Show();
			Window.BringToForeground();

			Handle = Window.Handle;

			logger.Debug("Initialized browser window.");
		}

		private void InitializeResponsibilities(
			DialogHandler dialogHandler,
			DisplayHandler displayHandler,
			DownloadHandler downloadHandler,
			IFileSystemDialog fileSystemDialog,
			JavaScriptDialogHandler javaScriptDialogHandler,
			KeyboardHandler keyboardHandler,
			RequestFilter requestFilter,
			RequestHandler requestHandler,
			ResourceHandler resourceHandler)
		{
			var controlResponsibility = new ControlResponsibility(context);
			var cookieResponsibility = new CookieResponsibility(context, integrations);
			var dialogResponsibility = new DialogResponsibility(context, dialogHandler, fileSystemDialog, javaScriptDialogHandler);
			var displayResponsibility = new DisplayResponsibility(context, displayHandler);
			var downloadResponsibility = new DownloadResponsibility(context, downloadHandler);
			var keyboardResponsibility = new KeyboardResponsibility(context, keyboardHandler);
			var lifeSpanResponsibility = new LifeSpanResponsibility(context);
			var requestResponsibility = new RequestResponsibility(context, requestFilter, requestHandler, resourceHandler);
			var zoomResponsibilty = new ZoomResponsibility(context, keyboardHandler);

			controlResponsibility.TitleChanged += (t) => TitleChanged?.Invoke(t);
			cookieResponsibility.UserIdentifierDetected += (i) => UserIdentifierDetected?.Invoke(i);
			displayResponsibility.Closed += (i) => Closed?.Invoke(i);
			displayResponsibility.IconChanged += (i) => IconChanged?.Invoke(i);
			displayResponsibility.LoseFocusRequested += (f) => LoseFocusRequested?.Invoke(f);
			downloadResponsibility.ConfigurationDownloadRequested += (f, a) => ConfigurationDownloadRequested?.Invoke(f, a);
			keyboardResponsibility.LoseFocusRequested += (f) => LoseFocusRequested?.Invoke(f);
			lifeSpanResponsibility.PopupRequested += (a) => PopupRequested?.Invoke(a);
			requestResponsibility.ResetRequested += () => ResetRequested?.Invoke();
			requestResponsibility.TerminationRequested += () => TerminationRequested?.Invoke();
			requestResponsibility.TitleChanged += (t) => TitleChanged?.Invoke(t);
			requestResponsibility.UserIdentifierDetected += (i) => UserIdentifierDetected?.Invoke(i);

			responsibilities = new ResponsibilityCollection<WindowTask>(logger, new WindowResponsibility[]
			{
				controlResponsibility,
				cookieResponsibility,
				dialogResponsibility,
				displayResponsibility,
				downloadResponsibility,
				keyboardResponsibility,
				lifeSpanResponsibility,
				requestResponsibility,
				zoomResponsibilty
			});
		}
	}
}
