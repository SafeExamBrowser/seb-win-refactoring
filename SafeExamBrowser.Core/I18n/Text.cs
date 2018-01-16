/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.I18n
{
	public class Text : IText
	{
		private IDictionary<TextKey, string> cache = new Dictionary<TextKey, string>();
		private ILogger logger;

		public Text(ILogger logger)
		{
			this.logger = logger;
		}

		public string Get(TextKey key)
		{
			return cache.ContainsKey(key) ? cache[key] : $"Could not find string for key '{key}'!";
		}

		public void Initialize(ITextResource resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException(nameof(resource));
			}

			try
			{
				cache = resource.LoadText() ?? new Dictionary<TextKey, string>();
			}
			catch (Exception e)
			{
				logger.Error("Failed to load text data from provided resource!", e);
			}
		}
	}
}
