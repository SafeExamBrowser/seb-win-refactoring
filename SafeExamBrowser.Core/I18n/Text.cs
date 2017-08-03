/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.I18n;

namespace SafeExamBrowser.Core.I18n
{
	public class Text : IText
	{
		private readonly IDictionary<TextKey, string> cache;

		public Text(ITextResource resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException(nameof(resource));
			}

			cache = resource.LoadText() ?? new Dictionary<TextKey, string>();
		}

		public string Get(TextKey key)
		{
			return cache.ContainsKey(key) ? cache[key] : $"Could not find string for key '{key}'!";
		}
	}
}
