/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Logging
{
	/// <summary>
	/// Defines all log levels supported by the application.
	/// </summary>
	public enum LogLevel
	{
		/// <summary>
		/// All messages are logged.
		/// </summary>
		Debug,

		/// <summary>
		/// Only messages with <see cref="Info"/>, <see cref="Warning"/> and <see cref="Error"/> are logged.
		/// </summary>
		Info,

		/// <summary>
		/// Only messages with <see cref="Warning"/> and <see cref="Error"/> are logged.
		/// </summary>
		Warning,

		/// <summary>
		/// Only messages with <see cref="Error"/> are logged.
		/// </summary>
		Error
	}
}
