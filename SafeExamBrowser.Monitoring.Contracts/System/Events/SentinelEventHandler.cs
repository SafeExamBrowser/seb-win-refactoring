/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Contracts.System.Events
{
	/// <summary>
	/// The default handler for events of the <see cref="ISystemSentinel"/>. Normally indicates that a configuration or state has changed.
	/// </summary>
	public delegate void SentinelEventHandler(SentinelEventArgs args);
}
