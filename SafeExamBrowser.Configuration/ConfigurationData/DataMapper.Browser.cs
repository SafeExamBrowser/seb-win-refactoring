/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapAllowNavigation(Settings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindowSettings.AllowBackwardNavigation = allow;
				settings.Browser.MainWindowSettings.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowNavigationAdditionalWindow(Settings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindowSettings.AllowBackwardNavigation = allow;
				settings.Browser.AdditionalWindowSettings.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowPageZoom(Settings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowPageZoom = allow;
			}
		}

		private void MapAllowPopups(Settings settings, object value)
		{
			if (value is bool block)
			{
				settings.Browser.AllowPopups = !block;
			}
		}

		private void MapAllowReload(Settings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindowSettings.AllowReloading = allow;
			}
		}

		private void MapAllowReloadAdditionalWindow(Settings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindowSettings.AllowReloading = allow;
			}
		}

		private void MapMainWindowMode(Settings settings, object value)
		{
			const int FULLSCREEN = 1;

			if (value is int mode)
			{
				settings.Browser.MainWindowSettings.FullScreenMode = mode == FULLSCREEN;
			}
		}

		private void MapShowReloadWarning(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.MainWindowSettings.ShowReloadWarning = show;
			}
		}

		private void MapShowReloadWarningAdditionalWindow(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.AdditionalWindowSettings.ShowReloadWarning = show;
			}
		}

		private void MapUserAgentMode(IDictionary<string, object> rawData, Settings settings)
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
