/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
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
		DateTime DateTime { get; }
		LogLevel Severity { get; }
		string Message { get; }
		int ThreadId { get; }
	}
}
