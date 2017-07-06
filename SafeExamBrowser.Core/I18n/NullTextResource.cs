/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Core.I18n
{
	class NullTextResource : ITextResource
	{
		public IDictionary<Key, string> LoadText()
		{
			return new Dictionary<Key, string>();
		}
	}
}
