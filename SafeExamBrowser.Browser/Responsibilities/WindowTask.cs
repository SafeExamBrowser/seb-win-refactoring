/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Browser.Responsibilities
{
	/// <summary>
	/// Defines all tasks assumed by the responsibilities of the <see cref="BrowserWindow"/>.
	/// </summary>
	internal enum WindowTask
	{
		/// <summary>
		/// Initiaties the traversal of all already existing browser cookies.
		/// </summary>
		InitiateCookieTraversal,

		/// <summary>
		/// Initializes the lifespan handler for a new browser window.
		/// </summary>
		InitializeLifeSpanHandler,

		/// <summary>
		/// Initializes the request filter for a new browser window.
		/// </summary>
		InitializeRequestFilter,

		/// <summary>
		/// Initializes the zoom level for a new browser window.
		/// </summary>
		InitializeZoom,

		/// <summary>
		/// Register all event handlers during window initialization.
		/// </summary>
		RegisterEvents
	}
}
