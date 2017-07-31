/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface
{
	public delegate void AddressChangedHandler(string address);
	public delegate void TitleChangedHandler(string title);

	public interface IBrowserControl
	{
		/// <summary>
		/// Event fired when the address of the browser control changes.
		/// </summary>
		event AddressChangedHandler AddressChanged;

		/// <summary>
		/// Event fired when the current page (and thus the title) of the browser control changes.
		/// </summary>
		event TitleChangedHandler TitleChanged;

		/// <summary>
		/// Navigates to the previous page in the browser control history.
		/// </summary>
		void NavigateBackwards();

		/// <summary>
		/// Navigates to the next page in the browser control history.
		/// </summary>
		void NavigateForwards();

		/// <summary>
		/// Navigates to the specified web address.
		/// </summary>
		void NavigateTo(string address);

		/// <summary>
		/// Reloads the current web page.
		/// </summary>
		void Reload();
	}
}
