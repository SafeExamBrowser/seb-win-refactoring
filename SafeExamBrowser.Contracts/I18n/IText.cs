/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.I18n
{
	/// <summary>
	/// Provides access to text data.
	/// </summary>
	public interface IText
	{
		/// <summary>
		/// Initializes the text module, e.g. loads text data from the specified text resource.
		/// </summary>
		void Initialize(ITextResource resource);

		/// <summary>
		/// Gets the text associated with the specified key. If the key was not found, a default text indicating
		/// that the given key is not configured will be returned.
		/// </summary>
		string Get(TextKey key);
	}
}
