/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapAllowAddressBar(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowAddressBar = allow;
			}
		}

		private void MapAllowAddressBarAdditionalWindow(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowAddressBar = allow;
			}
		}

		private void MapAllowConfigurationDownloads(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowConfigurationDownloads = allow;
			}
		}

		private void MapAllowDeveloperConsole(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowDeveloperConsole = allow;
				settings.Browser.AdditionalWindow.AllowDeveloperConsole = allow;
			}
		}

		private void MapAllowDownloads(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowDownloads = allow;
			}
		}

		private void MapAllowNavigation(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowBackwardNavigation = allow;
				settings.Browser.MainWindow.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowNavigationAdditionalWindow(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowBackwardNavigation = allow;
				settings.Browser.AdditionalWindow.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowPageZoom(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowPageZoom = allow;
			}
		}

		private void MapAllowPopups(ApplicationSettings settings, object value)
		{
			if (value is bool block)
			{
				settings.Browser.AllowPopups = !block;
			}
		}

		private void MapAllowReload(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowReloading = allow;
			}
		}

		private void MapAllowReloadAdditionalWindow(ApplicationSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowReloading = allow;
			}
		}

		private void MapEnableContentRequestFilter(ApplicationSettings settings, object value)
		{
			if (value is bool filter)
			{
				settings.Browser.Filter.FilterContentRequests = filter;
			}
		}

		private void MapEnableMainRequestFilter(ApplicationSettings settings, object value)
		{
			if (value is bool filter)
			{
				settings.Browser.Filter.FilterMainRequests = filter;
			}
		}

		private void MapUrlFilterRules(ApplicationSettings settings, object value)
		{
			const int ALLOW = 1;

			if (value is IList<object> ruleDataList)
			{
				foreach (var item in ruleDataList)
				{
					if (item is IDictionary<string, object> ruleData)
					{
						if (ruleData.TryGetValue(Keys.Browser.Filter.RuleIsActive, out var v) && v is bool active && active)
						{
							var rule = new FilterRuleSettings();

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleExpression, out v) && v is string expression)
							{
								rule.Expression = expression;
							}

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleAction, out v) && v is int action)
							{
								rule.Result = action == ALLOW ? FilterResult.Allow : FilterResult.Block;
							}

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleExpressionIsRegex, out v) && v is bool regex)
							{
								rule.Type = regex ? FilterType.Regex : FilterType.Simplified;
							}

							settings.Browser.Filter.Rules.Add(rule);
						}
					}
				}
			}
		}

		private void MapMainWindowMode(ApplicationSettings settings, object value)
		{
			const int FULLSCREEN = 1;

			if (value is int mode)
			{
				settings.Browser.MainWindow.FullScreenMode = mode == FULLSCREEN;
			}
		}

		private void MapShowReloadWarning(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.MainWindow.ShowReloadWarning = show;
			}
		}

		private void MapShowReloadWarningAdditionalWindow(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.AdditionalWindow.ShowReloadWarning = show;
			}
		}

		private void MapUserAgentMode(IDictionary<string, object> rawData, ApplicationSettings settings)
		{
			const int DEFAULT = 0;

			var useCustomForDesktop = rawData.TryGetValue(Keys.Browser.UserAgentModeDesktop, out var value) && value as int? != DEFAULT;
			var useCustomForMobile = rawData.TryGetValue(Keys.Browser.UserAgentModeMobile, out value) && value as int? != DEFAULT;

			if (settings.UserInterfaceMode == UserInterfaceMode.Desktop && useCustomForDesktop)
			{
				settings.Browser.UseCustomUserAgent = true;
				settings.Browser.CustomUserAgent = rawData[Keys.Browser.CustomUserAgentDesktop] as string;
			}
			else if (settings.UserInterfaceMode == UserInterfaceMode.Mobile && useCustomForMobile)
			{
				settings.Browser.UseCustomUserAgent = true;
				settings.Browser.CustomUserAgent = rawData[Keys.Browser.CustomUserAgentMobile] as string;
			}
		}
	}
}
