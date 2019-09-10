/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Text.RegularExpressions;

namespace SafeExamBrowser.Browser.Filters.Rules
{
	internal class RegexRule : Rule
	{
		private string expression;

		public RegexRule(string expression) : base(expression)
		{
		}

		protected override void Initialize(string expression)
		{
			this.expression = expression;
		}

		internal override bool IsMatch(string url)
		{
			return Regex.IsMatch(url, expression, RegexOptions.IgnoreCase);
		}
	}
}
