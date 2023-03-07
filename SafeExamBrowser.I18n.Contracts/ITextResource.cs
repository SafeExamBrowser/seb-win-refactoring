/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.I18n.Contracts
{
	/// <summary>
	/// Defines a text resource, i.e. a source from which text data can be loaded.
	/// </summary>
	public interface ITextResource
	{
		/// <summary>
		/// Loads all text data from a resource. Throws an exception if the data could not be loaded, e.g. due to a data format error.
		/// </summary>
		IDictionary<TextKey, string> LoadText();
	}
}
