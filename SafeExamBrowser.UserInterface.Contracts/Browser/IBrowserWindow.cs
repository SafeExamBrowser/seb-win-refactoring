/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts.Browser
{
	/// <summary>
	/// Defines the functionality of a browser window, i.e. a window with an embedded browser instance (see <see cref="IBrowserControl"/>).
	/// </summary>
	public interface IBrowserWindow : IWindow
	{
		/// <summary>
		/// Enables the backward navigation button.
		/// </summary>
		bool CanNavigateBackwards { set; }

		/// <summary>
		/// Enables the forward navigation button.
		/// </summary>
		bool CanNavigateForwards { set; }

		/// <summary>
		/// The native handle of the window.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		/// Event fired when the user changed the URL.
		/// </summary>
		event AddressChangedEventHandler AddressChanged;

		/// <summary>
		/// Event fired when the user would like to navigate backwards.
		/// </summary>
		event ActionRequestedEventHandler BackwardNavigationRequested;

		/// <summary>
		/// Event fired when the user would like to open the developer console.
		/// </summary>
		event ActionRequestedEventHandler DeveloperConsoleRequested;

		/// <summary>
		/// Event fired when the user would like to search the current page.
		/// </summary>
		event FindRequestedEventHandler FindRequested;

		/// <summary>
		/// Event fired when the user would like to navigate forwards.
		/// </summary>
		event ActionRequestedEventHandler ForwardNavigationRequested;

		/// <summary>
		/// Event fired when the user would like to navigate home.
		/// </summary>
		event ActionRequestedEventHandler HomeNavigationRequested;

		/// <summary>
		/// Event fired when the browser window wants to lose focus to the taskbar.
		/// </summary>
		event LoseFocusRequestedEventHandler LoseFocusRequested;

		/// <summary>
		/// Event fired when the user would like to reload the current page.
		/// </summary>
		event ActionRequestedEventHandler ReloadRequested;

		/// <summary>
		/// Event fired when the user would like to zoom in.
		/// </summary>
		event ActionRequestedEventHandler ZoomInRequested;

		/// <summary>
		/// Event fired when the user would like to zoom out.
		/// </summary>
		event ActionRequestedEventHandler ZoomOutRequested;

		/// <summary>
		/// Event fired when the user would like to reset the zoom factor.
		/// </summary>
		event ActionRequestedEventHandler ZoomResetRequested;

		/// <summary>
		/// Sets the focus on the address bar of the window.
		/// </summary>
		void FocusAddressBar();

		/// <summary>
		/// Sets the focus on the browser content of the window.
		/// </summary>
		void FocusBrowser();

		/// <summary>
		/// Sets the focus on the toolbar of the window. If the parameter is set to true, the first focusable control on the toolbar gets focused.
		/// If it is set to false, the last one.
		/// </summary>
		void FocusToolbar(bool forward);

		/// <summary>
		/// Displays the find toolbar to search the content of a page.
		/// </summary>
		void ShowFindbar();

		/// <summary>
		/// Updates the address bar of the browser window to the given value.
		/// </summary>
		void UpdateAddress(string adress);

		/// <summary>
		/// Updates the icon of the browser window.
		/// </summary>
		void UpdateIcon(IconResource icon);

		/// <summary>
		/// Updates the download state for the given item.
		/// </summary>
		void UpdateDownloadState(DownloadItemState state);

		/// <summary>
		/// Updates the loading state according to the given value.
		/// </summary>
		void UpdateLoadingState(bool isLoading);

		/// <summary>
		/// Updates the page load progress according to the given value.
		/// </summary>
		void UpdateProgress(double value);

		/// <summary>
		/// Sets the title of the browser window to the given value.
		/// </summary>
		void UpdateTitle(string title);

		/// <summary>
		/// Updates the display value of the current page zoom. Value is expected to be in percentage.
		/// </summary>
		void UpdateZoomLevel(double value);
	}
}
