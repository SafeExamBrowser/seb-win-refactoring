/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Filters.Rules
{
	internal class SimplifiedRule : IRule
	{
		private const string URL_DELIMITER_PATTERN = @"(?:([^\:]*)\:\/\/)?(?:([^\:\@]*)(?:\:([^\@]*))?\@)?(?:([^\/‌​\:]*))?(?:\:([0-9\*]*))?([^\?#]*)?(?:\?([^#]*))?(?:#(.*))?";

		private Regex fragment;
		private Regex host;
		private Regex password;
		private Regex path;
		private int? port;
		private Regex query;
		private Regex scheme;
		private Regex user;

		public FilterResult Result { get; private set; }

		public void Initialize(FilterRuleSettings settings)
		{
			ValidateExpression(settings.Expression);
			ParseExpression(settings.Expression);

			Result = settings.Result;
		}

		public bool IsMatch(Request request)
		{
			var url = new Uri(request.Url, UriKind.Absolute);
			var isMatch = true;

			//isMatch &= scheme == default(Regex) || ...;
			//isMatch &= user == default(Regex) || ...;
			//isMatch &= password == default(Regex) || ...;
			isMatch &= host.IsMatch(url.Host);
			isMatch &= !port.HasValue || port == url.Port;
			//isMatch &= path == default(Regex) || ...;
			//isMatch &= query == default(Regex) || ...;
			//isMatch &= fragment == default(Regex) || ...;

			return isMatch;
		}

		private void ParseExpression(string expression)
		{
			var match = Regex.Match(expression, URL_DELIMITER_PATTERN);

			//ParseScheme(match.Groups[1].Value);
			//ParseUser(match.Groups[2].Value);
			//ParsePassword(match.Groups[3].Value);
			ParseHost(match.Groups[4].Value);
			ParsePort(match.Groups[5].Value);
			//ParsePath(match.Groups[6].Value);
			//ParseQuery(match.Groups[7].Value);
			//ParseFragment(match.Groups[8].Value);
		}

		private void ParseHost(string expression)
		{
			var hasToplevelDomain = Regex.IsMatch(expression, @"\.+");
			var hasSubdomain = Regex.IsMatch(expression, @"\.{2,}");
			var allowOnlyExactSubdomain = expression.StartsWith(".");

			if (allowOnlyExactSubdomain)
			{
				expression = expression.Substring(1);
			}

			expression = Regex.Escape(expression);
			expression = ReplaceWildcard(expression);

			if (!hasToplevelDomain)
			{
				expression = $@"{expression}(\.[a-z]+)";
			}

			if (!hasSubdomain && !allowOnlyExactSubdomain)
			{
				expression = $@"(.+?\.)*{expression}";
			}

			host = Build(expression);
		}

		private void ParsePort(string expression)
		{
			if (int.TryParse(expression, out var port))
			{
				this.port = port;
			}
		}

		private Regex Build(string expression)
		{
			return new Regex($"^{expression}$", RegexOptions.IgnoreCase);
		}

		private string ReplaceWildcard(string expression)
		{
			return expression.Replace(@"\*", ".*?");
		}

		private void ValidateExpression(string expression)
		{
			if (expression == default(string))
			{
				throw new ArgumentNullException(nameof(expression));
			}

			if (!Regex.IsMatch(expression, @"[a-zA-Z0-9\*]+"))
			{
				throw new ArgumentException("Expression must consist of at least one alphanumeric character or asterisk!", nameof(expression));
			}

			try
			{
				Regex.Match(expression, URL_DELIMITER_PATTERN);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Expression is not a valid simplified filter expression!", nameof(expression), e);
			}
		}
	}
}
