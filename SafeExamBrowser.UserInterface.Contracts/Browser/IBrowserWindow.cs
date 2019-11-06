/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts;
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
		/// Event fired when the user would like to navigate forwards.
		/// </summary>
		event ActionRequestedEventHandler ForwardNavigationRequested;

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
		/// Updates the address bar of the browser window to the given value.
		/// </summary>
		void UpdateAddress(string adress);

		/// <summary>
		/// Updates the icon of the browser window.
		/// </summary>
		void UpdateIcon(IconResource icon);

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
