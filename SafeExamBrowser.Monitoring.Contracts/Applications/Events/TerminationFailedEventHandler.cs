/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Monitoring.Contracts.Applications.Events
{
	/// <summary>
	/// Indicates that the given blacklisted applications could not be terminated.
	/// </summary>
	public delegate void TerminationFailedEventHandler(IEnumerable<RunningApplication> applications);
}
