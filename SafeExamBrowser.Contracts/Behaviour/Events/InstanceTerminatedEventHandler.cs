/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */


namespace SafeExamBrowser.Contracts.Behaviour.Events
{
	/// <summary>
	/// Event handler used to indicate that an application instance with a particular ID has terminated.
	/// </summary>
	public delegate void InstanceTerminatedEventHandler(InstanceIdentifier id);
}
