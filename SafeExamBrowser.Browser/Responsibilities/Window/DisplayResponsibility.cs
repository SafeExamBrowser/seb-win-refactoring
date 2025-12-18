/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class DisplayResponsibility : WindowResponsibility
	{
		private readonly DisplayHandler displayHandler;
		private readonly HttpClient httpClient;

		private (string term, bool isInitial, bool caseSensitive, bool forward) findParameters;

		internal event WindowClosedEventHandler Closed;
		internal event IconChangedEventHandler IconChanged;
		internal event LoseFocusRequestedEventHandler LoseFocusRequested;

		public DisplayResponsibility(BrowserWindowContext context, DisplayHandler displayHandler) : base(context)
		{
			this.displayHandler = displayHandler;
			this.httpClient = new HttpClient();
		}

		public override void Assume(WindowTask task)
		{
			if (task == WindowTask.RegisterEvents)
			{
				RegisterEvents();
			}
		}

		private void RegisterEvents()
		{
			displayHandler.FaviconChanged += DisplayHandler_FaviconChanged;
			displayHandler.ProgressChanged += DisplayHandler_ProgressChanged;

			Window.AddressChanged += Window_AddressChanged;
			Window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			Window.Closed += Window_Closed;
			Window.Closing += Window_Closing;
			Window.DeveloperConsoleRequested += Window_DeveloperConsoleRequested;
			Window.FindRequested += Window_FindRequested;
			Window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
			Window.HomeNavigationRequested += HomeNavigationRequested;
			Window.LoseFocusRequested += Window_LoseFocusRequested;
			Window.ReloadRequested += ReloadRequested;
		}

		private void DisplayHandler_FaviconChanged(string uri)
		{
			Task.Run(() =>
			{
				var request = new HttpRequestMessage(HttpMethod.Head, uri);
				var response = httpClient.SendAsync(request).ContinueWith(task =>
				{
					if (task.IsCompleted && task.Result.IsSuccessStatusCode)
					{
						Context.Icon = new BrowserIconResource(uri);

						IconChanged?.Invoke(Context.Icon);
						Window.UpdateIcon(Context.Icon);
					}
				});
			});
		}

		private void DisplayHandler_ProgressChanged(double value)
		{
			Window.UpdateProgress(value);
		}

		private void Window_AddressChanged(string address)
		{
			var isValid = Uri.TryCreate(address, UriKind.Absolute, out _) || Uri.TryCreate($"https://{address}", UriKind.Absolute, out _);

			if (isValid)
			{
				Logger.Debug($"The user requested to navigate to '{address}', the URI is valid.");
				Control.NavigateTo(address);
			}
			else
			{
				Logger.Debug($"The user requested to navigate to '{address}', but the URI is not valid.");
				Window.UpdateAddress(string.Empty);
			}
		}

		private void Window_BackwardNavigationRequested()
		{
			Logger.Debug("Navigating backwards...");
			Control.NavigateBackwards();
		}

		private void Window_Closing()
		{
			Logger.Debug($"Window is closing...");
		}

		private void Window_Closed()
		{
			Logger.Debug($"Window has been closed.");
			Control.Destroy();
			Closed?.Invoke(Context.Id);
		}

		private void Window_DeveloperConsoleRequested()
		{
			Logger.Debug("Showing developer console...");
			Control.ShowDeveloperConsole();
		}

		private void Window_FindRequested(string term, bool isInitial, bool caseSensitive, bool forward = true)
		{
			if (Settings.AllowFind)
			{
				findParameters.caseSensitive = caseSensitive;
				findParameters.forward = forward;
				findParameters.isInitial = isInitial;
				findParameters.term = term;

				Control.Find(term, isInitial, caseSensitive, forward);
			}
		}

		private void Window_ForwardNavigationRequested()
		{
			Logger.Debug("Navigating forwards...");
			Control.NavigateForwards();
		}

		private void Window_LoseFocusRequested(bool forward)
		{
			LoseFocusRequested?.Invoke(forward);
		}
	}
}
