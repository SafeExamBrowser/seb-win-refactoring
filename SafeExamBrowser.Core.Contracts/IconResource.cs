/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Core.Contracts
{
	/// <summary>
	/// Defines an icon resource, i.e. the path to and type of an icon.
	/// </summary>
	public class IconResource
	{
		/// <summary>
		/// Defines the data type of the resource.
		/// </summary>
		public IconResourceType Type { get; set; }

		/// <summary>
		/// The <see cref="System.Uri"/> pointing to the icon data.
		/// </summary>
		public Uri Uri { get; set; }
	}
}
