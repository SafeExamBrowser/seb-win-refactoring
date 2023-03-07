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
	/// Provides functionality to save configuration data as a particular resource type.
	/// </summary>
	public interface IResourceSaver
	{
		/// <summary>
		/// Indicates whether data can be saved as the specified resource.
		/// </summary>
		bool CanSave(Uri destination);

		/// <summary>
		/// Tries to save the configuration data as the specified resource.
		/// </summary>
		SaveStatus TrySave(Uri destination, Stream data);
	}
}
