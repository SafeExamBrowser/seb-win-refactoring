/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Defines the data format for a configuration file.
	/// </summary>
	public interface IDataFormat
	{
		/// <summary>
		/// Indicates whether the given data complies with the required format.
		/// </summary>
		bool CanParse(byte[] data);

		/// <summary>
		/// Attempts to parse the given binary data.
		/// </summary>
		LoadStatus TryParse(byte[] data, out Settings.Settings settings, string adminPassword = null, string settingsPassword = null);
	}
}
