/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Defines a window of an <see cref="IApplication"/>.
	/// </summary>
	public interface IApplicationWindow
	{
		/// <summary>
		/// The native handle of the window.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		/// The icon of the window.
		/// </summary>
		IconResource Icon { get; }

		/// <summary>
		/// The title of the window.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Event fired when the icon of the window has changed.
		/// </summary>
		event IconChangedEventHandler IconChanged;

		/// <summary>
		/// Event fired when the title of the window has changed.
		/// </summary>
		event TitleChangedEventHandler TitleChanged;

		/// <summary>
		/// Brings the window to the foreground and activates it.
		/// </summary>
		void Activate();
	}
}
