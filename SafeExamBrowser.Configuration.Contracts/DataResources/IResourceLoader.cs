/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;

namespace SafeExamBrowser.Configuration.Contracts.DataResources
{
	/// <summary>
	/// Provides functionality to load configuration data from a particular resource type.
	/// </summary>
	public interface IResourceLoader
	{
		/// <summary>
		/// Indicates whether data can be loaded from the specified resource.
		/// </summary>
		bool CanLoad(Uri resource);

		/// <summary>
		/// Tries to load the configuration data from the specified resource.
		/// </summary>
		LoadStatus TryLoad(Uri resource, out Stream data);
	}
}
