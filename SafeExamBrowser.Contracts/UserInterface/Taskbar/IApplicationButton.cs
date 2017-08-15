/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public delegate void ApplicationButtonClickedEventHandler(Guid? instanceId = null);

	public interface IApplicationButton
	{
		/// <summary>
		/// Event fired when the user clicked on the application button. If multiple instances of an application are active,
		/// the handler is only executed when the user selects one of the instances.
		/// </summary>
		event ApplicationButtonClickedEventHandler Clicked;

		/// <summary>
		/// Registers a new instance of an application, to be started / displayed if the user clicked the taskbar button.
		/// </summary>
		void RegisterInstance(IApplicationInstance instance);
	}
}
