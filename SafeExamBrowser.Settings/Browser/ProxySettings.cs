/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Settings.Browser.Proxy;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines the proxy settings for the browser engine.
	/// </summary>
	[Serializable]
	public class ProxySettings
	{
		/// <summary>
		/// Determines whether proxy auto-configuration should be used. Requires a valid URL defined in <see cref="AutoConfigureUrl"/>.
		/// </summary>
		public bool AutoConfigure { get; set; }

		/// <summary>
		/// A valid URL to a proxy auto-configuration file (.pac). Is only evaluated if <see cref="AutoConfigure"/> is enabled.
		/// </summary>
		public string AutoConfigureUrl { get; set; }

		/// <summary>
		/// Forces proxy auto-detection by the browser engine.
		/// </summary>
		public bool AutoDetect { get; set; }

		/// <summary>
		/// A list of hosts for which all proxy settings should be bypassed.
		/// </summary>
		public IList<string> BypassList { get; set; }

		/// <summary>
		/// The proxy policy to be used.
		/// </summary>
		public ProxyPolicy Policy { get; set; }

		/// <summary>
		/// Defines all proxies to be used.
		/// </summary>
		public IList<ProxyConfiguration> Proxies { get; set; }

		public ProxySettings()
		{
			BypassList = new List<string>();
			Proxies = new List<ProxyConfiguration>();
		}
	}
}
