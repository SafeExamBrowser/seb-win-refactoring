/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.Filters
{
	internal class RuleFactory : IRuleFactory
	{
		public IRule CreateRule(FilterRuleType type)
		{
			switch (type)
			{
				case FilterRuleType.Regex:
					return new RegexRule();
				case FilterRuleType.Simplified:
					return new SimplifiedRule();
				default:
					throw new NotImplementedException($"Filter rule of type '{type}' is not yet implemented!");
			}
		}
	}
}
