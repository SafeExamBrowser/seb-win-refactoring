/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar.Events
{
	/// <summary>
	/// Indicates that an <see cref="IApplicationButton"/> has been clicked, optionally specifying the ID of the selected instance (if
	/// multiple instances of the same application are running).
	/// </summary>
	public delegate void ApplicationButtonClickedEventHandler(Guid? instanceId = null);
}
