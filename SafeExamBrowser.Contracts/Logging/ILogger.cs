/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.Logging
{
	/// <summary>
	/// Defines the functionality of the logger.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Logs the given message with severity <b>DEBUG</b>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Debug(string message);

		/// <summary>
		/// Logs the given message with severity <b>INFO</b>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Info(string message);

		/// <summary>
		/// Logs the given message with severity <b>WARNING</b>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Warn(string message);

		/// <summary>
		/// Logs the given message with severity <b>ERROR</b>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Error(string message);

		/// <summary>
		/// Logs the given message with severity <b>ERROR</b> and includes information about
		/// the specified exception (i.e. type, message and stacktrace).
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Error(string message, Exception exception);

		/// <summary>
		/// Logs the given message as raw text.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Log(string message);

		/// <summary>
		/// Appends the given content to the log.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Log(ILogContent content);

		/// <summary>
		/// Suscribes an observer to the application log.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Subscribe(ILogObserver observer);

		/// <summary>
		/// Unsubscribes an observer from the application log.
		/// </summary>
		void Unsubscribe(ILogObserver observer);

		/// <summary>
		/// Returns a copy of the current log content.
		/// </summary>
		IList<ILogContent> GetLog();
	}
}
