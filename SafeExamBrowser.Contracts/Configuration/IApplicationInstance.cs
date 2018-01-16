/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Configuration
{
	public delegate void TerminatedEventHandler(Guid id);
	public delegate void NameChangedEventHandler(string name);

	public interface IApplicationInstance
	{
		/// <summary>
		/// The unique identifier for the application instance.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The name or (document) title of the application instance.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Event fired when the application instance has been terminated.
		/// </summary>
		event TerminatedEventHandler Terminated;

		/// <summary>
		/// Event fired when the name or (document) title of the application instance has changed.
		/// </summary>
		event NameChangedEventHandler NameChanged;

		/// <summary>
		/// The main window of the application instance.
		/// </summary>
		IWindow Window { get; }
	}
}
