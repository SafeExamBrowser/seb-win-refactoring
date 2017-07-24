/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Configuration
{
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
		/// The main window of the application instance.
		/// </summary>
		IWindow Window { get; }

		/// <summary>
		/// Registers the given window as the main window of the application instance.
		/// </summary>
		void RegisterWindow(IWindow window);
	}
}
