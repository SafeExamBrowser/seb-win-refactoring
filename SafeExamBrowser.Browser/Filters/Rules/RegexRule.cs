/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Text.RegularExpressions;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Filters.Rules
{
	internal class RegexRule : IRule
	{
		private string expression;

		public FilterResult Result { get; private set; }

		public void Initialize(FilterRuleSettings settings)
		{
			expression = settings.Expression;
			Result = settings.Result;
		}

		public bool IsMatch(Request request)
		{
			return Regex.IsMatch(request.Url, expression, RegexOptions.IgnoreCase);
		}
	}
}
