/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Core.I18n
{
	public static class Strings
	{
		private static IDictionary<Key, string> cache = new Dictionary<Key, string>();

		public static void Initialize(IStringResource resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException(nameof(resource));
			}

			cache = resource.LoadStrings();
		}

		public static string Get(Key key)
		{
			return cache.ContainsKey(key) ? cache[key] : $"Could not find string for key '{key}'!";
		}
	}
}
