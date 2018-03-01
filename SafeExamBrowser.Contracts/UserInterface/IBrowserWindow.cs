/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface
{
	public delegate void ActionRequestedEventHandler();

	public interface IBrowserWindow : IWindow
	{
		/// <summary>
		/// Event fired when the user changed the URL.
		/// </summary>
		event AddressChangedEventHandler AddressChanged;

		/// <summary>
		/// Event fired when the user would like to navigate backwards.
		/// </summary>
		event ActionRequestedEventHandler BackwardNavigationRequested;

		/// <summary>
		/// Event fired when the user would like to navigate forwards.
		/// </summary>
		event ActionRequestedEventHandler ForwardNavigationRequested;

		/// <summary>
		/// Event fired when the user would like to reload the current page.
		/// </summary>
		event ActionRequestedEventHandler ReloadRequested;

		/// <summary>
		/// Determines whether this window is the main browser window.
		/// </summary>
		bool IsMainWindow { get; set; }

		/// <summary>
		/// Updates the address bar of the browser window to the given value;
		/// </summary>
		void UpdateAddress(string adress);

		/// <summary>
		/// Updates the loading state according to the given value.
		/// </summary>
		void UpdateLoadingState(bool isLoading);

		/// <summary>
		/// Sets the title of the browser window to the given value;
		/// </summary>
		void UpdateTitle(string title);
	}
}
