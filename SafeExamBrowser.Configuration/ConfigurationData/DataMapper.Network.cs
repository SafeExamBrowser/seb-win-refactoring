/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Network.Contracts.Filter;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapEnableContentRequestFilter(Settings settings, object value)
		{
			if (value is bool enable)
			{
				settings.Network.EnableContentRequestFilter = enable;
			}
		}

		private void MapEnableMainRequestFilter(Settings settings, object value)
		{
			if (value is bool enable)
			{
				settings.Network.EnableMainRequestFilter = enable;
			}
		}

		private void MapUrlFilterRules(Settings settings, object value)
		{
			const int ALLOW = 1;

			if (value is IEnumerable<IDictionary<string, object>> ruleDataList)
			{
				foreach (var ruleData in ruleDataList)
				{
					if (ruleData.TryGetValue(Keys.Network.Filter.RuleIsActive, out var v) && v is bool active && active)
					{
						var rule = new FilterRule();

						if (ruleData.TryGetValue(Keys.Network.Filter.RuleExpression, out v) && v is string expression)
						{
							rule.Expression = expression;
						}

						if (ruleData.TryGetValue(Keys.Network.Filter.RuleAction, out v) && v is int action)
						{
							rule.Result = action == ALLOW ? FilterResult.Allow : FilterResult.Block ;
						}

						if (ruleData.TryGetValue(Keys.Network.Filter.RuleExpressionIsRegex, out v) && v is bool regex)
						{
							rule.Type = regex ? FilterType.Regex : FilterType.Simplified;
						}

						settings.Network.FilterRules.Add(rule);
					}
				}
			}
		}
	}
}
