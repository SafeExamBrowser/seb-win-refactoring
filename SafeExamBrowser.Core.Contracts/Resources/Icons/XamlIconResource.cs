/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Core.Contracts.Resources.Icons
{
	/// <summary>
	/// Defines an icon resource which consists of XAML markup (i.e. vector graphics).
	/// </summary>
	public class XamlIconResource : IconResource
	{
		/// <summary>
		/// The <see cref="System.Uri"/> pointing to the XAML file.
		/// </summary>
		public Uri Uri { get; set; }
	}
}
