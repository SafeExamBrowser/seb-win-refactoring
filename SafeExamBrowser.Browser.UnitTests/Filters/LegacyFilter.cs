/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla internal
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SafeExamBrowser.Browser.UnitTests.Filters
{
	internal class LegacyFilter
	{
		internal Regex scheme;
		internal Regex user;
		internal Regex password;
		internal Regex host;
		internal int? port;
		internal Regex path;
		internal Regex query;
		internal Regex fragment;

		internal LegacyFilter(string filterExpressionString)
		{
			SEBURLFilterExpression URLFromString = new SEBURLFilterExpression(filterExpressionString);
			try
			{
				this.scheme = RegexForFilterString(URLFromString.scheme);
				this.user = RegexForFilterString(URLFromString.user);
				this.password = RegexForFilterString(URLFromString.password);
				this.host = RegexForHostFilterString(URLFromString.host);
				this.port = URLFromString.port;
				this.path = RegexForPathFilterString(URLFromString.path);
				this.query = RegexForQueryFilterString(URLFromString.query);
				this.fragment = RegexForFilterString(URLFromString.fragment);
			}
			catch (Exception)
			{
				throw;
			}
		}

		// Method comparing all components of a passed URL with the filter expression
		// and returning YES (= allow or block) if it matches
		internal bool IsMatch(Uri URLToFilter)
		{
			Regex filterComponent;

			// If a scheme is indicated in the filter expression, it has to match
			filterComponent = scheme;
			UriBuilder urlToFilterParts = new UriBuilder(URLToFilter);

			if (filterComponent != null &&
				!Regex.IsMatch(URLToFilter.Scheme, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				// Scheme of the URL to filter doesn't match the one from the filter expression: Exit with matching = NO
				return false;
			}

			string userInfo = URLToFilter.UserInfo;
			filterComponent = user;
			if (filterComponent != null &&
				!Regex.IsMatch(urlToFilterParts.UserName, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			filterComponent = password;
			if (filterComponent != null &&
				!Regex.IsMatch(urlToFilterParts.Password, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			filterComponent = host;
			if (filterComponent != null &&
				!Regex.IsMatch(URLToFilter.Host, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			if (port != null && URLToFilter.Port != port)
			{
				return false;
			}

			filterComponent = path;
			if (filterComponent != null &&
				!Regex.IsMatch(URLToFilter.AbsolutePath.TrimEnd(new char[] { '/' }), filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			string urlQuery = URLToFilter.GetComponents(UriComponents.Query, UriFormat.Unescaped);
			filterComponent = query;
			if (filterComponent != null)
			{
				// If there's a query filter component, then we need to even filter empty URL query strings
				// as the filter might either allow some specific queries or no query at all ("?." query filter)
				if (urlQuery == null)
				{
					urlQuery = "";
				}
				if (!Regex.IsMatch(urlQuery, filterComponent.ToString(), RegexOptions.IgnoreCase))
				{
					return false;
				}
			}

			string urlFragment = URLToFilter.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
			filterComponent = fragment;
			if (filterComponent != null &&
				!Regex.IsMatch(urlFragment, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			// URL matches the filter expression
			return true;
		}

		internal static Regex RegexForFilterString(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				return null;
			}
			else
			{
				string regexString = Regex.Escape(filterString);
				regexString = regexString.Replace("\\*", ".*?");
				// Add regex command characters for matching at start and end of a line (part)
				regexString = string.Format("^{0}$", regexString);

				try
				{
					Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);
					return regex;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		internal static Regex RegexForHostFilterString(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				return null;
			}
			else
			{
				try
				{
					// Check if host string has a dot "." prefix to disable subdomain matching
					if (filterString.Length > 1 && filterString.StartsWith("."))
					{
						// Get host string without the "." prefix
						filterString = filterString.Substring(1);
						// Get regex for host <*://example.com> (without possible subdomains)
						return RegexForFilterString(filterString);
					}
					// Allow subdomain matching: Create combined regex for <example.com> and <*.example.com>
					string regexString = Regex.Escape(filterString);
					regexString = regexString.Replace("\\*", ".*?");
					// Add regex command characters for matching at start and end of a line (part)
					regexString = string.Format("^(({0})|(.*?\\.{0}))$", regexString);
					Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);
					return regex;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		internal static Regex RegexForPathFilterString(string filterString)
		{
			// Trim a possible trailing slash "/", we will instead add a rule to also match paths to directories without trailing slash
			filterString = filterString.TrimEnd(new char[] { '/' });
			;

			if (string.IsNullOrEmpty(filterString))
			{
				return null;
			}
			else
			{
				try
				{
					// Check if path string ends with a "/*" for matching contents of a directory
					if (filterString.EndsWith("/*"))
					{
						// As the path filter string matches for a directory, we need to add a string to match directories without trailing slash

						// Get path string without the "/*" suffix
						string filterStringDirectory = filterString.Substring(0, filterString.Length - 2);

						string regexString = Regex.Escape(filterString);
						regexString = regexString.Replace("\\*", ".*?");

						string regexStringDir = Regex.Escape(filterString);
						regexStringDir = regexStringDir.Replace("\\*", ".*?");

						// Add regex command characters for matching at start and end of a line (part)
						regexString = string.Format("^(({0})|({1}))$", regexString, regexStringDir);

						Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);
						return regex;
					}
					else
					{
						return RegexForFilterString(filterString);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		internal static Regex RegexForQueryFilterString(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				return null;
			}
			else
			{
				if (filterString.Equals("."))
				{
					// Add regex command characters for matching at start and end of a line (part)
					// and regex for no string allowed
					string regexString = @"^$";
					try
					{
						Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);
						return regex;
					}
					catch (Exception)
					{
						throw;
					}
				}
				else
				{
					return RegexForFilterString(filterString);
				}
			}
		}

		public override string ToString()
		{
			StringBuilder expressionString = new StringBuilder();
			string part;
			expressionString.Append("^");

			/// Scheme
			if (this.scheme != null)
			{
				// If there is a regex filter for scheme
				// get stripped regex pattern
				part = StringForRegexFilter(this.scheme);
			}
			else
			{
				// otherwise use the regex wildcard pattern for scheme
				part = @".*?";
			}

			expressionString.AppendFormat("{0}:\\/\\/", part);

			/// User/Password
			if (this.user != null)
			{
				part = StringForRegexFilter(this.user);

				expressionString.Append(part);

				if (this.password != null)
				{
					expressionString.AppendFormat(":{0}@", StringForRegexFilter(this.password));
				}
				else
				{
					expressionString.Append("@");
				}
			}

			/// Host
			string hostPort = "";
			if (this.host != null)
			{
				hostPort = StringForRegexFilter(this.host);
			}
			else
			{
				hostPort = ".*?";
			}

			/// Port
			if (this.port != null && this.port > 0 && this.port <= 65535)
			{
				hostPort = string.Format("{0}:{1}", hostPort, this.port);
			}

			// When there is a host, but no path
			if (this.host != null && this.path == null)
			{
				hostPort = string.Format("(({0})|({0}\\/.*?))", hostPort);
			}

			expressionString.Append(hostPort);

			/// Path
			if (this.path != null)
			{
				string path = StringForRegexFilter(this.path);
				if (path.StartsWith("\\/"))
				{
					expressionString.Append(path);
				}
				else
				{
					expressionString.AppendFormat("\\/{0}", path);
				}
			}

			/// Query
			if (this.query != null)
			{
				// Check for special case Query = "?." which means no query string is allowed
				if (StringForRegexFilter(this.query).Equals("."))
				{
					expressionString.AppendFormat("[^\\?]");
				}
				else
				{
					expressionString.AppendFormat("\\?{0}", StringForRegexFilter(this.query));
				}
			}
			else
			{
				expressionString.AppendFormat("(()|(\\?.*?))");
			}

			/// Fragment
			if (this.fragment != null)
			{
				expressionString.AppendFormat("#{0}", StringForRegexFilter(this.fragment));
			}

			expressionString.Append("$");

			return expressionString.ToString();
		}

		internal string StringForRegexFilter(Regex regexFilter)
		{
			// Get pattern string from regular expression
			string regexPattern = regexFilter.ToString();
			if (regexPattern.Length <= 2)
			{
				return "";
			}
			// Remove the regex command characters for matching at start and end of a line
			regexPattern = regexPattern.Substring(1, regexPattern.Length - 2);
			return regexPattern;
		}

		private class SEBURLFilterExpression
		{
			internal string scheme;
			internal string user;
			internal string password;
			internal string host;
			internal int? port;
			internal string path;
			internal string query;
			internal string fragment;

			internal SEBURLFilterExpression(string filterExpressionString)
			{
				if (!string.IsNullOrEmpty(filterExpressionString))
				{
					/// Convert Uri to a SEBURLFilterExpression
					string splitURLRegexPattern = @"(?:([^\:]*)\:\/\/)?(?:([^\:\@]*)(?:\:([^\@]*))?\@)?(?:([^\/‌​\:]*))?(?:\:([0-9\*]*))?([^\?#]*)?(?:\?([^#]*))?(?:#(.*))?";
					Regex splitURLRegex = new Regex(splitURLRegexPattern);
					Match regexMatch = splitURLRegex.Match(filterExpressionString);
					if (regexMatch.Success == false)
					{
						return;
					}

					this.scheme = regexMatch.Groups[1].Value;
					this.user = regexMatch.Groups[2].Value;
					this.password = regexMatch.Groups[3].Value;
					this.host = regexMatch.Groups[4].Value;

					// Treat a special case when a query or fragment is interpreted as part of the host address
					if (this.host.Contains("?") || this.host.Contains("#"))
					{
						string splitURLRegexPattern2 = @"([^\?#]*)?(?:\?([^#]*))?(?:#(.*))?";
						Regex splitURLRegex2 = new Regex(splitURLRegexPattern2);
						Match regexMatch2 = splitURLRegex2.Match(this.host);
						if (regexMatch.Success == false)
						{
							return;
						}
						this.host = regexMatch2.Groups[1].Value;
						this.port = null;
						this.path = "";
						this.query = regexMatch2.Groups[2].Value;
						this.fragment = regexMatch2.Groups[3].Value;
					}
					else
					{
						string portNumber = regexMatch.Groups[5].Value;

						// We only want a port if the filter expression string explicitely defines one!
						if (portNumber.Length == 0 || portNumber == "*")
						{
							this.port = null;
						}
						else
						{
							this.port = UInt16.Parse(portNumber);
						}

						this.path = regexMatch.Groups[6].Value.TrimEnd(new char[] { '/' });
						this.query = regexMatch.Groups[7].Value;
						this.fragment = regexMatch.Groups[8].Value;
					}
				}
			}

			internal static string User(string userInfo)
			{
				string user = "";
				if (!string.IsNullOrEmpty(userInfo))
				{
					int userPasswordSeparator = userInfo.IndexOf(":");
					if (userPasswordSeparator == -1)
					{
						user = userInfo;
					}
					else
					{
						if (userPasswordSeparator != 0)
						{
							user = userInfo.Substring(0, userPasswordSeparator);
						}
					}
				}
				return user;
			}

			internal static string Password(string userInfo)
			{
				string password = "";
				if (!string.IsNullOrEmpty(userInfo))
				{
					int userPasswordSeparator = userInfo.IndexOf(":");
					if (userPasswordSeparator != -1)
					{
						if (userPasswordSeparator < userInfo.Length - 1)
						{
							password = userInfo.Substring(userPasswordSeparator + 1, userInfo.Length - 1 - userPasswordSeparator);
						}
					}
				}
				return password;
			}

			internal SEBURLFilterExpression(string scheme, string user, string password, string host, int port, string path, string query, string fragment)
			{
				this.scheme = scheme;
				this.user = user;
				this.password = password;
				this.host = host;
				this.port = port;
				this.path = path;
				this.query = query;
				this.fragment = fragment;
			}

			public override string ToString()
			{
				StringBuilder expressionString = new StringBuilder();
				if (!string.IsNullOrEmpty(this.scheme))
				{
					if (!string.IsNullOrEmpty(this.host))
					{
						expressionString.AppendFormat("{0}://", this.scheme);
					}
					else
					{
						expressionString.AppendFormat("{0}:", this.scheme);
					}
				}
				if (!string.IsNullOrEmpty(this.user))
				{
					expressionString.Append(this.user);

					if (!string.IsNullOrEmpty(this.password))
					{
						expressionString.AppendFormat(":{0}@", this.password);
					}
					else
					{
						expressionString.Append("@");
					}
				}
				if (!string.IsNullOrEmpty(this.host))
				{
					expressionString.Append(this.host);
				}
				if (this.port != null && this.port > 0 && this.port <= 65535)
				{
					expressionString.AppendFormat(":{0}", this.port);
				}
				if (!string.IsNullOrEmpty(this.path))
				{
					if (this.path.StartsWith("/"))
					{
						expressionString.Append(this.path);
					}
					else
					{
						expressionString.AppendFormat("/{0}", this.path);
					}
				}
				if (!string.IsNullOrEmpty(this.query))
				{
					expressionString.AppendFormat("?{0}", this.query);
				}
				if (!string.IsNullOrEmpty(this.fragment))
				{
					expressionString.AppendFormat("#{0}", this.fragment);
				}

				return expressionString.ToString();
			}
		}
	}
}
