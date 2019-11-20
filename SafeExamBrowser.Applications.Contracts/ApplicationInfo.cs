/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// The information about an application which can be accessed via the shell.
	/// </summary>
	public class ApplicationInfo
	{
		/// <summary>
		/// Indicates whether the application should be automatically started.
		/// </summary>
		public bool AutoStart { get; set; }

		/// <summary>
		/// The name of the application.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The tooltip for the application.
		/// </summary>
		public string Tooltip { get; set; }

		/// <summary>
		/// The resource providing the application icon.
		/// </summary>
		public IconResource Icon { get; set; }
	}
}
