using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SebWindowsConfig.Utilities
{
	public class SEBURLFilterRegexExpression
	{
		public Regex scheme;
		public Regex user;
		public Regex password;
		public Regex host;
		public int? port;
		public Regex path;
		public Regex query;
		public Regex fragment;


		public SEBURLFilterRegexExpression(string filterExpressionString)
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


		public static Regex RegexForFilterString(string filterString)
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


		public static Regex RegexForHostFilterString(string filterString)
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


		public static Regex RegexForPathFilterString(string filterString)
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


		public static Regex RegexForQueryFilterString(string filterString)
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
			} else
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
				} else
				{
					expressionString.AppendFormat("\\?{0}", StringForRegexFilter(this.query));
				}
			} else
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



		public string StringForRegexFilter(Regex regexFilter)
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

	}
}