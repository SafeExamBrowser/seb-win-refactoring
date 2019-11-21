using System;
using System.Text;
using System.Text.RegularExpressions;
using DictObj = System.Collections.Generic.Dictionary<string, object>;
using ListObj = System.Collections.Generic.List<object>;

namespace SebWindowsConfig.Utilities
{
	public class SEBURLFilter
	{
		public bool enableURLFilter;
		public bool enableContentFilter;
		public ListObj permittedList = new ListObj();
		public ListObj prohibitedList = new ListObj();

		// Updates filter rule arrays with current settings
		public void UpdateFilterRules()
		{
			if (prohibitedList.Count != 0)
			{
				prohibitedList.Clear();
			}

			if (permittedList.Count != 0)
			{
				permittedList.Clear();
			}

			enableURLFilter = (bool)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable];
			enableContentFilter = (bool)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnableContentFilter];

			// Add global URLFilterRules
			ListObj URLFilterRules = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterRules];
			ReadURLFilterRules(URLFilterRules);

			// Add URLFilterRules from additional resources
			ListObj additionalResources = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources];
			ReadFilterRulesFromAdditionalResources(additionalResources);

			// If URL filtering is enabled, then
			// check if Start URL gets allowed by current filter rules and if not add a rule for the Start URL
			string startURLString = (string)SEBSettings.settingsCurrent[SEBSettings.KeyStartURL];

			if (enableURLFilter && Uri.TryCreate(startURLString, UriKind.Absolute, out Uri startURL))
			{
				if (TestURLAllowed(startURL) != URLFilterRuleActions.allow)
				{
					SEBURLFilterRegexExpression expression;
					// If Start URL is not allowed: Create one using the full Start URL
					try
					{
						expression = new SEBURLFilterRegexExpression(startURLString);
					}
					catch (Exception ex)
					{
						Logger.AddError("Could not create SEBURLFilterRegexExpression: ", this, ex, ex.Message);
						prohibitedList.Clear();
						permittedList.Clear();
						// Convert these rules and add them to the XULRunner seb keys
						CreateSebRuleLists();
						return;
					}

					// Add this Start URL filter expression to the permitted filter list
					permittedList.Add(expression);

				}
			}
			// Convert these rules and add them to the XULRunner seb keys
			CreateSebRuleLists();
		}


		public void ReadURLFilterRules(ListObj URLFilterRules)
		{
			foreach (DictObj URLFilterRule in URLFilterRules)
			{

				if (URLFilterRule.ContainsKey(SEBSettings.KeyURLFilterRuleRegex) && (bool)URLFilterRule[SEBSettings.KeyURLFilterRuleActive] == true)
				{

					string expressionString = (string)URLFilterRule[SEBSettings.KeyURLFilterRuleExpression];
					if (!String.IsNullOrEmpty(expressionString))
					{
						Object expression;

						bool regex = (bool)URLFilterRule[SEBSettings.KeyURLFilterRuleRegex];
						try
						{
							if (regex)
							{
								expression = new Regex(expressionString, RegexOptions.IgnoreCase);
							}
							else
							{
								expression = new SEBURLFilterRegexExpression(expressionString);
							}
						}
						catch (Exception ex)
						{
							Logger.AddError("Could not create SEBURLFilterRegexExpression: ", this, ex, ex.Message);
							prohibitedList.Clear();
							permittedList.Clear();
							throw;
						}

						int action = (int)URLFilterRule[SEBSettings.KeyURLFilterRuleAction];
						switch (action)
						{

							case (int)URLFilterRuleActions.block:

								prohibitedList.Add(expression);
								break;


							case (int)URLFilterRuleActions.allow:

								permittedList.Add(expression);
								break;
						}
					}
				}
			}
		}


		// Read URLFilterRules from additionalResources
		public void ReadFilterRulesFromAdditionalResources(ListObj additionalResources)
		{
			foreach (DictObj additionalResource in additionalResources)
			{
				if ((bool)additionalResource[SEBSettings.KeyAdditionalResourcesActive])
				{
					object URLFilterRules;
					if (additionalResource.TryGetValue(SEBSettings.KeyURLFilterRules, out URLFilterRules))
					{
						ReadURLFilterRules((ListObj)URLFilterRules);
					}

					// Are there further additional resources in this additional resource?
					if (additionalResource.TryGetValue(SEBSettings.KeyAdditionalResources, out object additionalSubResources))
					{
						if (((ListObj)additionalSubResources).Count != 0)
						{
							ReadFilterRulesFromAdditionalResources((ListObj)additionalSubResources);
						}
					}
				}
			}
		}

		// Convert these rules and add them to the XULRunner seb keys
		public void CreateSebRuleLists()
		{
			// Set prohibited rules
			SEBSettings.settingsCurrent[SEBSettings.KeyUrlFilterBlacklist] = SebRuleStringForSEBURLFilterRuleList(prohibitedList);

			// Set permitted rules
			SEBSettings.settingsCurrent[SEBSettings.KeyUrlFilterWhitelist] = SebRuleStringForSEBURLFilterRuleList(permittedList);

			// All rules are regex
			SEBSettings.settingsCurrent[SEBSettings.KeyUrlFilterRulesAsRegex] = true;

			// Set if content filter is enabled
			SEBSettings.settingsCurrent[SEBSettings.KeyUrlFilterTrustedContent] = !(bool)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnableContentFilter];
		}


		public string SebRuleStringForSEBURLFilterRuleList(ListObj filterRuleList)
		{
			if (filterRuleList.Count == 0)
			{
				// No rules defined
				return "";
			}

			StringBuilder sebRuleString = new StringBuilder();
			foreach (object expression in filterRuleList)
			{
				if (expression != null)
				{
					if (sebRuleString.Length == 0)
					{
						sebRuleString.Append(expression.ToString());
					}
					else
					{
						sebRuleString.AppendFormat(";{0}", expression.ToString());
					}
				}
			}

			return sebRuleString.ToString();
		}


		// Filter URL and return if it is allowed or blocked
		public URLFilterRuleActions TestURLAllowed(Uri URLToFilter)
		{
			string URLToFilterString = URLToFilter.ToString();
			// By default URLs are blocked
			bool allowURL = false;
			bool blockURL = false;

			/// Apply current filter rules (expressions/actions) to URL
			/// Apply prohibited filter expressions

			foreach (object expression in prohibitedList)
			{

				if (expression.GetType().Equals(typeof(Regex)))
				{
					if (Regex.IsMatch(URLToFilterString, expression.ToString()))
					{
						blockURL = true;
						break;
					}
				}

				if (expression.GetType().Equals(typeof(SEBURLFilterRegexExpression)))
				{
					if (URLMatchesFilterExpression(URLToFilter, (SEBURLFilterRegexExpression)expression))
					{
						blockURL = true;
						break;
					}
				}
			}
			if (blockURL == true)
			{
				return URLFilterRuleActions.block;
			}

			/// Apply permitted filter expressions

			foreach (object expression in permittedList)
			{

				if (expression.GetType().Equals(typeof(Regex)))
				{
					if (Regex.IsMatch(URLToFilterString, expression.ToString()))
					{
						allowURL = true;
						break;
					}
				}

				if (expression.GetType().Equals(typeof(SEBURLFilterRegexExpression)))
				{
					if (URLMatchesFilterExpression(URLToFilter, (SEBURLFilterRegexExpression)expression))
					{
						allowURL = true;
						break;
					}
				}

			}
			// Return URLFilterActionAllow if URL is allowed or
			// URLFilterActionUnknown if it's unknown (= it will anyways be blocked)
			return allowURL ? URLFilterRuleActions.allow : URLFilterRuleActions.unknown;
		}

		// Method comparing all components of a passed URL with the filter expression
		// and returning YES (= allow or block) if it matches
		public bool URLMatchesFilterExpression(Uri URLToFilter, SEBURLFilterRegexExpression filterExpression)
		{
			Regex filterComponent;

			// If a scheme is indicated in the filter expression, it has to match
			filterComponent = filterExpression.scheme;
			UriBuilder urlToFilterParts = new UriBuilder(URLToFilter);

			if (filterComponent != null &&
				!Regex.IsMatch(URLToFilter.Scheme, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				// Scheme of the URL to filter doesn't match the one from the filter expression: Exit with matching = NO
				return false;
			}

			string userInfo = URLToFilter.UserInfo;
			filterComponent = filterExpression.user;
			if (filterComponent != null && 
				!Regex.IsMatch(urlToFilterParts.UserName, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			filterComponent = filterExpression.password;
			if (filterComponent != null && 
				!Regex.IsMatch(urlToFilterParts.Password, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			filterComponent = filterExpression.host;
			if (filterComponent != null && 
				!Regex.IsMatch(URLToFilter.Host, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			if (filterExpression.port != null && URLToFilter.Port != filterExpression.port)
			{
				return false;
			}

			filterComponent = filterExpression.path;
			if (filterComponent != null && 
				!Regex.IsMatch(URLToFilter.AbsolutePath.TrimEnd(new char[] { '/' }), filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			string urlQuery = URLToFilter.GetComponents(UriComponents.Query, UriFormat.Unescaped);
			filterComponent = filterExpression.query;
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
			filterComponent = filterExpression.fragment;
			if (filterComponent != null &&
				!Regex.IsMatch(urlFragment, filterComponent.ToString(), RegexOptions.IgnoreCase))
			{
				return false;
			}

			// URL matches the filter expression
			return true;
		}

	}
}
