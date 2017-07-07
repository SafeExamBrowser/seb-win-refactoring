/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.Logging
{
	public interface ILogger
	{
		void Info(string message);
		void Warn(string message);
		void Error(string message);
		void Error(string message, Exception exception);
		void Log(string message);
		void Log(ILogContent content);
		void Subscribe(ILogObserver observer);
		void Unsubscribe(ILogObserver observer);
		IList<ILogContent> GetLog();
	}
}
