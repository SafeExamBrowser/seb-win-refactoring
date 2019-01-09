/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// The information about a (third-party) application which can be accessed via the <see cref="UserInterface.Taskbar.ITaskbar"/>.
	/// </summary>
	public interface IApplicationInfo
	{
		/// <summary>
		/// The name of the application.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The tooltip for the application.
		/// </summary>
		string Tooltip { get; }

		/// <summary>
		/// The resource providing the application icon.
		/// </summary>
		IIconResource IconResource { get; }
	}
}
