/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Defines an instance of a (third-party) application which can be accessed via the shell.
	/// </summary>
	public interface IApplicationInstance
	{
		/// <summary>
		/// The unique identifier for the application instance.
		/// </summary>
		InstanceIdentifier Id { get; }

		/// <summary>
		/// The name or (document) title of the application instance.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Event fired when the icon of the application instance has changed.
		/// </summary>
		event IconChangedEventHandler IconChanged;

		/// <summary>
		/// Event fired when the application instance has been terminated.
		/// </summary>
		event InstanceTerminatedEventHandler Terminated;

		/// <summary>
		/// Event fired when the name or (document) title of the application instance has changed.
		/// </summary>
		event NameChangedEventHandler NameChanged;

		// TODO
		///// <summary>
		///// The main window of the application instance.
		///// </summary>
		//IWindow Window { get; }
	}
}
