﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Logging.Contracts
{
	/// <summary>
	/// Defines an observer of the application log which can subscribe to a logger via <see cref="ILogger.Subscribe(ILogObserver)"/>.
	/// </summary>
	public interface ILogObserver
	{
		/// <summary>
		/// Notifies an observer once new content has been added to the application log.
		/// </summary>
		void Notify(ILogContent content);
	}
}
