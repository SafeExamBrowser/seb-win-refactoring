/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Browser
{
	/// <summary>
	/// Defines the functionality of a browser control (i.e. an instance of the browser resp. its user interface) and is normally embedded
	/// within an <see cref="IBrowserWindow"/>.
	/// </summary>
	public interface IBrowserControl
	{
		/// <summary>
		/// The address which is currently loaded.
		/// </summary>
		string Address { get; }

		/// <summary>
		/// Indicates whether a backward navigation can be performed.
		/// </summary>
		bool CanNavigateBackwards { get; }

		/// <summary>
		/// Indicates whether a forward navigation can be performed.
		/// </summary>
		bool CanNavigateForwards { get; }

		/// <summary>
		/// The user interface control to be embedded in an <see cref="IBrowserWindow"/>.
		/// </summary>
		object EmbeddableControl { get; }

		/// <summary>
		/// Event fired when the address of the browser control changes.
		/// </summary>
		event AddressChangedEventHandler AddressChanged;

		/// <summary>
		/// Event fired when a load error occurs.
		/// </summary>
		event LoadFailedEventHandler LoadFailed;

		/// <summary>
		/// Event fired when the loading state of the browser control changes.
		/// </summary>
		event LoadingStateChangedEventHandler LoadingStateChanged;

		/// <summary>
		/// Event fired when the current page (and thus the title) of the browser control changes.
		/// </summary>
		event TitleChangedEventHandler TitleChanged;

		/// <summary>
		/// Finalizes the browser control (e.g. stops audio / video playback) and releases all used resources.
		/// </summary>
		void Destroy();

		/// <summary>
		/// Executes the given JavaScript code in the browser.
		/// </summary>
		void ExecuteJavascript(string code, Action<JavascriptResult> callback);

		/// <summary>
		/// Attempts to find the given term on the current page according to the specified parameters.
		/// </summary>
		void Find(string term, bool isInitial, bool caseSensitive, bool forward = true);

		/// <summary>
		/// Initializes the browser control.
		/// </summary>
		void Initialize();

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
		/// Opens the developer console or actives it, if it is already open.
		/// </summary>
		void ShowDeveloperConsole();

		/// <summary>
		/// Reloads the current web page.
		/// </summary>
		void Reload();

		/// <summary>
		/// Sets the page zoom to the given level. A value of <c>0</c> resets the page zoom.
		/// </summary>
		void Zoom(double level);
	}
}
