/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Provides functionality to parse configuration data with a particular format.
	/// </summary>
	public interface IDataFormat
	{
		/// <summary>
		/// Indicates whether the given data complies with the required format.
		/// </summary>
		bool CanParse(Stream data);

		/// <summary>
		/// Tries to parse the given data, using the optional password. As long as the result is not <see cref="LoadStatus.Success"/>,
		/// the referenced settings may be <c>null</c> or in an undefinable state!
		/// </summary>
		LoadStatus TryParse(Stream data, out Settings.Settings settings, string password = null);
	}
}
