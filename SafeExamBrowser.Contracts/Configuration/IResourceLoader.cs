/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Loads configuration data from various sources (e.g. the internet) and provides related resource handling functionality.
	/// </summary>
	public interface IResourceLoader
	{
		/// <summary>
		/// Indicates whether the given <see cref="Uri"/> identifies a HTML resource.
		/// </summary>
		bool IsHtmlResource(Uri resource);
	}
}
