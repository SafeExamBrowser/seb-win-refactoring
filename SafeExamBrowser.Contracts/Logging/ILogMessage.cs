/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Logging
{
	public interface ILogMessage : ILogContent
	{
		/// <summary>
		/// The date when the message was logged.
		/// </summary>
		DateTime DateTime { get; }

		/// <summary>
		/// The severity of the message.
		/// </summary>
		LogLevel Severity { get; }

		/// <summary>
		/// The message itself.
		/// </summary>
		string Message { get; }

		/// <summary>
		/// Information about the thread on which the message was logged.
		/// </summary>
		IThreadInfo ThreadInfo { get; }
	}
}
