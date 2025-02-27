/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Security
{
	/// <summary>
	/// Defines a restriction for the SEB version to be used.
	/// </summary>
	[Serializable]
	public class VersionRestriction
	{
		/// <summary>
		/// The major version to be used.
		/// </summary>
		public int Major { get; set; }

		/// <summary>
		/// The minor version to be used.
		/// </summary>
		public int Minor { get; set; }

		/// <summary>
		/// Optionally defines the patch version to be used.
		/// </summary>
		public int? Patch { get; set; }

		/// <summary>
		/// Optionally defines the build version to be used.
		/// </summary>
		public int? Build { get; set; }

		/// <summary>
		/// Determines whether the restriction defines the minimum version to be used.
		/// </summary>
		public bool IsMinimumRestriction { get; set; }

		/// <summary>
		/// Determines whether the restriction requires the usage of the Alliance Edition.
		/// </summary>
		public bool RequiresAllianceEdition { get; set; }

		public override string ToString()
		{
			return $"{Major}.{Minor}{(Patch.HasValue ? $".{Patch}" : "")}{(Build.HasValue ? $".{Build}" : "")}{(RequiresAllianceEdition ? ".AE" : "")}{(IsMinimumRestriction ? ".min" : "")}";
		}
	}
}
