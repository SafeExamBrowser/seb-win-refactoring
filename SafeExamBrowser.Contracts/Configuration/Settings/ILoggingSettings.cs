/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface ILoggingSettings
	{
		/// <summary>
		/// The point in time when the application was started.
		/// </summary>
		DateTime ApplicationStartTime { get; }

		/// <summary>
		/// The file path under which the log of the browser component is to be stored.
		/// </summary>
		string BrowserLogFile { get; }

		/// <summary>
		/// The file path under which the log of the client component is to be stored.
		/// </summary>
		string ClientLogFile { get; }

		/// <summary>
		/// The file path under which the log of the runtime component is to be stored.
		/// </summary>
		string RuntimeLogFile { get; }
	}
}
