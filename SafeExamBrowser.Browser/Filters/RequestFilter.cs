/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.Filters
{
	internal class RequestFilter : IRequestFilter
	{
		private IList<IRule> allowRules;
		private IList<IRule> blockRules;

		public FilterResult Default { get; set; }

		internal RequestFilter()
		{
			allowRules = new List<IRule>();
			blockRules = new List<IRule>();
			Default = FilterResult.Block;
		}

		public void Load(IRule rule)
		{
			switch (rule.Result)
			{
				case FilterResult.Allow:
					allowRules.Add(rule);
					break;
				case FilterResult.Block:
					blockRules.Add(rule);
					break;
				default:
					throw new NotImplementedException($"Filter processing for result '{rule.Result}' is not yet implemented!");
			}
		}

		public FilterResult Process(Request request)
		{
			foreach (var rule in blockRules)
			{
				if (rule.IsMatch(request))
				{
					return FilterResult.Block;
				}
			}

			foreach (var rule in allowRules)
			{
				if (rule.IsMatch(request))
				{
					return FilterResult.Allow;
				}
			}

			return Default;
		}
	}
}
