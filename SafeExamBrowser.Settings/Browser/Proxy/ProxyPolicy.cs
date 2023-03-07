/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Browser.Proxy
{
	/// <summary>
	/// Defines all currently supported proxy policies for the browser.
	/// </summary>
	public enum ProxyPolicy
	{
		/// <summary>
		/// Use custom proxy settings as defined in <see cref="ProxySettings"/>.
		/// </summary>
		Custom,

		/// <summary>
		/// Use the proxy settings of the operating system (i.e. ignore all custom settings defined in <see cref="ProxySettings"/>).
		/// </summary>
		System
	}
}
