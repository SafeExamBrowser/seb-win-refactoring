/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.Resources.Icons
{
	/// <summary>
	/// Defines an icon resource which is a file with embedded icon data (e.g. an executable).
	/// </summary>
	public class EmbeddedIconResource : IconResource
	{
		/// <summary>
		/// The full path of the file.
		/// </summary>
		public string FilePath { get; set; }
	}
}
