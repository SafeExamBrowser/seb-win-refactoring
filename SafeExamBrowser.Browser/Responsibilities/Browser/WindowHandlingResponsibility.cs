/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class WindowHandlingResponsibility : BrowserResponsibility
	{
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly IKeyGenerator keyGenerator;
		private readonly IMessageBox messageBox;
		private readonly INativeMethods nativeMethods;
		private readonly SessionMode sessionMode;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private int counter = default;

		private IList<BrowserWindow> Windows => Context.Windows;

		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event LoseFocusRequestedEventHandler LoseFocusRequested;
		internal event TerminationRequestedEventHandler TerminationRequested;
		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;
		internal event WindowsChangedEventHandler WindowsChanged;

		public WindowHandlingResponsibility(
			BrowserApplicationContext context,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			IKeyGenerator keyGenerator,
			IMessageBox messageBox,
			INativeMethods nativeMethods,
			SessionMode sessionMode,
			IText text,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.keyGenerator = keyGenerator;
			this.messageBox = messageBox;
			this.nativeMethods = nativeMethods;
			this.sessionMode = sessionMode;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public override void Assume(BrowserTask task)
		{
			switch (task)
			{
				case BrowserTask.CreateMainWindow:
					CreateNewWindow();
					break;
				case BrowserTask.CloseAllWindows:
					CloseAllWindows();
					break;
			}
		}

		private void CloseAllWindows()
		{
			foreach (var window in Windows)
			{
				window.Closed -= Window_Closed;
				window.Close();

				Logger.Info($"Closed browser window #{window.Id}.");
			}
		}

		private void CreateNewWindow(PopupRequestedEventArgs args = default)
		{
			var id = ++counter;
			var startUrl = GenerateStartUrl();
			var windowContext = new BrowserWindowContext
			{
				Logger = Logger.CloneFor($"Browser Window #{id}"),
				HashAlgorithm = hashAlgorithm,
				Icon = new BrowserIconResource(),
				Id = id,
				IsMainWindow = Windows.Count == 0,
				MessageBox = messageBox,
				Settings = Settings,
				StartUrl = startUrl,
				Text = text,
				UserInterfaceFactory = uiFactory
			};
			var window = new BrowserWindow(AppConfig, windowContext, fileSystemDialog, keyGenerator, sessionMode);

			window.Closed += Window_Closed;
			window.ConfigurationDownloadRequested += (f, a) => ConfigurationDownloadRequested?.Invoke(f, a);
			window.PopupRequested += Window_PopupRequested;
			window.ResetRequested += Window_ResetRequested;
			window.UserIdentifierDetected += (i) => UserIdentifierDetected?.Invoke(i);
			window.TerminationRequested += () => TerminationRequested?.Invoke();
			window.LoseFocusRequested += (forward) => LoseFocusRequested?.Invoke(forward);

			window.Initialize();
			Windows.Add(window);

			if (args != default)
			{
				args.Window = window;
			}
			else
			{
				window.Show();
			}

			Logger.Info($"Created browser window #{window.Id}.");
			WindowsChanged?.Invoke();
		}

		private string GenerateStartUrl()
		{
			var url = Settings.StartUrl;

			if (Settings.UseQueryParameter)
			{
				if (url.Contains("?") && Settings.StartUrlQuery?.Length > 1 && Uri.TryCreate(url, UriKind.Absolute, out var uri))
				{
					url = url.Replace(uri.Query, $"{uri.Query}&{Settings.StartUrlQuery.Substring(1)}");
				}
				else
				{
					url = $"{url}{Settings.StartUrlQuery}";
				}
			}

			return url;
		}

		private void Window_Closed(int id)
		{
			Windows.Remove(Windows.First(i => i.Id == id));
			WindowsChanged?.Invoke();
			Logger.Info($"Window #{id} has been closed.");
		}

		private void Window_PopupRequested(PopupRequestedEventArgs args)
		{
			Logger.Info($"Received request to create new window...");
			CreateNewWindow(args);
		}

		private void Window_ResetRequested()
		{
			Logger.Info("Attempting to reset browser...");

			BrowserApplication.AwaitReady();

			foreach (var window in Windows)
			{
				window.Closed -= Window_Closed;
				window.Close();
				Logger.Info($"Closed browser window #{window.Id}.");
			}

			Windows.Clear();
			WindowsChanged?.Invoke();

			if (Settings.DeleteCookiesOnStartup && Settings.DeleteCookiesOnShutdown)
			{
				// DeleteCookies();
				Context.Responsibilities.Delegate(BrowserTask.DeleteCookies);
			}

			nativeMethods.EmptyClipboard();
			CreateNewWindow();

			Logger.Info("Successfully reset browser.");
		}
	}
}
