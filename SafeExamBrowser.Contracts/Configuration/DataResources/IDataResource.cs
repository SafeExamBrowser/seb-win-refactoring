/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;

namespace SafeExamBrowser.Contracts.Configuration.DataResources
{
	/// <summary>
	/// Provides functionality to load and save configuration data from / as a particular resource.
	/// </summary>
	public interface IDataResource
	{
		/// <summary>
		/// Indicates whether data can be loaded from the specified resource.
		/// </summary>
		bool CanLoad(Uri resource);

		/// <summary>
		/// Tries to load the configuration data from the specified resource.
		/// </summary>
		LoadStatus TryLoad(Uri resource, out Stream data);

		/// <summary>
		/// Tries to save the given configuration data as the specified resource.
		/// </summary>
		SaveStatus TrySave(Uri resource, Stream data);
	}
}
