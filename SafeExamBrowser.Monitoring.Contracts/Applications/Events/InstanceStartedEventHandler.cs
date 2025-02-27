/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Contracts.Applications.Events
{
	/// <summary>
	/// Indicates that a new instance of a whitelisted application has been started.
	/// </summary>
	public delegate void InstanceStartedEventHandler(Guid applicationId, IProcess process);
}
