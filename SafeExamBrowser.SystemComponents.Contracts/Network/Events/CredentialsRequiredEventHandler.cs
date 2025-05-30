﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Network.Events
{
	/// <summary>
	/// Indicates that credentials are required to connect to a network.
	/// </summary>
	public delegate void CredentialsRequiredEventHandler(CredentialsRequiredEventArgs args);
}
