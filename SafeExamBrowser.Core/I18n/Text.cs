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
	public class Text
	{
		private static Text instance;
		private static readonly object @lock = new object();

		private IDictionary<Key, string> cache = new Dictionary<Key, string>();

		public Text(ITextResource resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException(nameof(resource));
			}

			cache = resource.LoadText();
		}

		public static Text Instance
		{
			get
			{
				lock (@lock)
				{
					return instance ?? new Text(new NullTextResource());
				}
			}
			set
			{
				lock (@lock)
				{
					instance = value ?? throw new ArgumentNullException(nameof(value));
				}
			}
		}

		public string Get(Key key)
		{
			return cache.ContainsKey(key) ? cache[key] : $"Could not find string for key '{key}'!";
		}
	}
}
