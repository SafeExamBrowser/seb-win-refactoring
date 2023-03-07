/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Logging.Contracts
{
	/// <summary>
	/// Defines the functionality of the logger.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// The currently active severity threshold. All messages with a lower severity won't be logged!
		/// </summary>
		LogLevel LogLevel { get; set; }

		/// <summary>
		/// Logs the given message with severity <see cref="LogLevel.Debug"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Debug(string message);

		/// <summary>
		/// Logs the given message with severity <see cref="LogLevel.Info"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Info(string message);

		/// <summary>
		/// Logs the given message with severity <see cref="LogLevel.Warning"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Warn(string message);

		/// <summary>
		/// Logs the given message with severity <see cref="LogLevel.Error"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Error(string message);

		/// <summary>
		/// Logs the given message with severity <see cref="LogLevel.Error"/> and includes
		/// information about the specified exception (i.e. type, message and stacktrace).
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Error(string message, Exception exception);

		/// <summary>
		/// Logs the given message as raw text.
		/// </summary>
		/// <exception cref="ArgumentNullException" />
		void Log(string message);

		/// <summary>
		/// Subscribes an observer to the application log.
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
