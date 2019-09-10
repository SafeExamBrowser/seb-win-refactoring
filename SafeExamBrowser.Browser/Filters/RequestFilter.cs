/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Filters
{
	internal class RequestFilter
	{
		private IList<Rule> allowRules;
		private IList<Rule> blockRules;

		internal FilterResult Default { get; set; }

		internal RequestFilter()
		{
			allowRules = new List<Rule>();
			blockRules = new List<Rule>();
			Default = FilterResult.Block;
		}

		internal void Load(FilterRuleSettings settings)
		{
			var rule = default(Rule);

			switch (settings.Type)
			{
				case FilterType.Regex:
					rule = new RegexRule(settings.Expression);
					break;
				case FilterType.Simplified:
					rule = new SimpleRule(settings.Expression);
					break;
				default:
					throw new NotImplementedException($"Filter rule of type '{settings.Type}' is not yet implemented!");
			}

			switch (settings.Result)
			{
				case FilterResult.Allow:
					allowRules.Add(rule);
					break;
				case FilterResult.Block:
					blockRules.Add(rule);
					break;
				default:
					throw new NotImplementedException($"Filter result '{settings.Result}' is not yet implemented!");
			}
		}

		internal FilterResult Process(string url)
		{
			foreach (var rule in blockRules)
			{
				if (rule.IsMatch(url))
				{
					return FilterResult.Block;
				}
			}

			foreach (var rule in allowRules)
			{
				if (rule.IsMatch(url))
				{
					return FilterResult.Allow;
				}
			}

			return Default;
		}
	}
}
