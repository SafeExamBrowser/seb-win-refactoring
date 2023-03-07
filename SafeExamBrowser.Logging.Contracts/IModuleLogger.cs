/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Logging.Contracts
{
	/// <summary>
	/// An implementation of <see cref="ILogger"/> which automatically appends information about the module from which messages are logged.
	/// </summary>
	public interface IModuleLogger : ILogger
	{
		/// <summary>
		/// Creates a new instance which will automatically append the given module information.
		/// </summary>
		IModuleLogger CloneFor(string moduleInfo);
	}
}
