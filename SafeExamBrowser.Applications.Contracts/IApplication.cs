/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Controls the lifetime and functionality of an application.
	/// </summary>
	public interface IApplication
	{
		/// <summary>
		/// Indicates whether the application should be automatically started.
		/// </summary>
		bool AutoStart { get; }

		/// <summary>
		/// The resource providing the application icon.
		/// </summary>
		IconResource Icon { get; }

		/// <summary>
		/// The unique identifier of the application.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The name of the application.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The tooltip for the application.
		/// </summary>
		string Tooltip { get; }

		/// <summary>
		/// Event fired when the windows of the application have changed.
		/// </summary>
		event WindowsChangedEventHandler WindowsChanged;

		/// <summary>
		/// Returns all windows of the application.
		/// </summary>
		IEnumerable<IApplicationWindow> GetWindows();

		/// <summary>
		/// Performs any initialization work, if necessary.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Starts the execution of the application.
		/// </summary>
		void Start();

		/// <summary>
		/// Performs any termination work, e.g. releasing of used resources.
		/// </summary>
		void Terminate();
	}
}
