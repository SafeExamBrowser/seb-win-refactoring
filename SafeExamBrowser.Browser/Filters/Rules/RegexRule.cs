/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.Filters.Rules
{
	internal class RegexRule : IRule
	{
		private string expression;

		public FilterResult Result { get; private set; }

		public void Initialize(FilterRuleSettings settings)
		{
			ValidateExpression(settings.Expression);

			expression = settings.Expression;
			Result = settings.Result;
		}

		public bool IsMatch(Request request)
		{
			return Regex.IsMatch(request.Url, expression, RegexOptions.IgnoreCase);
		}

		private void ValidateExpression(string expression)
		{
			if (expression == default(string))
			{
				throw new ArgumentNullException(nameof(expression));
			}

			try
			{
				Regex.Match("", expression);
			}
			catch (Exception e)
			{
				throw new ArgumentException($"Invalid regular expression!", nameof(expression), e);
			}
		}
	}
}
