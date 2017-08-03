/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.I18n
{
	public interface ITextResource
	{
		/// <summary>
		/// Loads all text data from a resource.
		/// </summary>
		IDictionary<TextKey, string> LoadText();
	}
}
