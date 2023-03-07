/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.I18n.Contracts
{
	/// <summary>
	/// Provides access to text data.
	/// </summary>
	public interface IText
	{
		/// <summary>
		/// Initializes the text module, i.e. loads text data according to the currently active UI culture.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Gets the text associated with the specified key. If the key was not found, an error message indicating the missing key will be returned.
		/// </summary>
		string Get(TextKey key);
	}
}
