/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class ControlResponsibility : WindowResponsibility
	{
		internal event TitleChangedEventHandler TitleChanged;

		public ControlResponsibility(BrowserWindowContext context) : base(context)
		{
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
			Control.AddressChanged += Control_AddressChanged;
			Control.LoadFailed += Control_LoadFailed;
			Control.LoadingStateChanged += Control_LoadingStateChanged;
			Control.TitleChanged += Control_TitleChanged;
		}

		private void Control_AddressChanged(string address)
		{
			Logger.Info($"Navigated{(WindowSettings.UrlPolicy.CanLog() ? $" to '{address}'" : "")}.");

			Context.Url = address;
			Window.UpdateAddress(address);

			if (WindowSettings.UrlPolicy == UrlPolicy.Always || WindowSettings.UrlPolicy == UrlPolicy.BeforeTitle)
			{
				Context.Title = address;
				Window.UpdateTitle(address);
				TitleChanged?.Invoke(address);
			}
		}

		private void Control_LoadFailed(int errorCode, string errorText, bool isMainRequest, string url)
		{
			switch (errorCode)
			{
				case (int) CefErrorCode.Aborted:
					Logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} was aborted.");
					break;
				case (int) CefErrorCode.InternetDisconnected:
					Logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} has failed due to loss of internet connection.");
					break;
				case (int) CefErrorCode.None:
					Logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} was successful.");
					break;
				case (int) CefErrorCode.UnknownUrlScheme:
					Logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} has an unknown URL scheme and will be handled by the OS.");
					break;
				default:
					HandleUnknownLoadFailure(errorCode, errorText, isMainRequest, url);
					break;
			}
		}

		private void HandleUnknownLoadFailure(int errorCode, string errorText, bool isMainRequest, string url)
		{
			var requestInfo = $"{errorText} ({errorCode}, {(isMainRequest ? "main" : "resource")} request)";

			Logger.Warn($"Request{(WindowSettings.UrlPolicy.CanLogError() ? $" for '{url}'" : "")} failed: {requestInfo}.");

			if (isMainRequest)
			{
				var title = Text.Get(TextKey.Browser_LoadErrorTitle);
				var message = Text.Get(TextKey.Browser_LoadErrorMessage).Replace("%%URL%%", WindowSettings.UrlPolicy.CanLogError() ? url : "") + $" {requestInfo}";

				Task.Run(() => MessageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: Window)).ContinueWith(_ => Control.NavigateBackwards());
			}
		}

		private void Control_LoadingStateChanged(bool isLoading)
		{
			Window.CanNavigateBackwards = WindowSettings.AllowBackwardNavigation && Control.CanNavigateBackwards;
			Window.CanNavigateForwards = WindowSettings.AllowForwardNavigation && Control.CanNavigateForwards;
			Window.UpdateLoadingState(isLoading);
		}

		private void Control_TitleChanged(string title)
		{
			if (WindowSettings.UrlPolicy != UrlPolicy.Always)
			{
				Context.Title = title;
				Window.UpdateTitle(Context.Title);
				TitleChanged?.Invoke(Context.Title);
			}
		}
	}
}
