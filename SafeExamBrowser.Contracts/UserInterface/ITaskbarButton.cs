/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.UserInterface
{
	public delegate void TaskbarButtonClickHandler(Guid? instanceId = null);

	public interface ITaskbarButton
	{
		/// <summary>
		/// OnClick handler, executed when the user clicks on the application button. If multiple instances of
		/// an application are active, the handler is only executed when the user selects one of the instances.
		/// </summary>
		event TaskbarButtonClickHandler OnClick;

		/// <summary>
		/// Registers a new instance of an application, to be displayed if the user clicks the taskbar button
		/// when there are already one or more instances of the same application running.
		/// </summary>
		void RegisterInstance(IApplicationInstance instance);

		/// <summary>
		/// Unregisters an application instance, e.g. if it gets closed.
		/// </summary>
		/// <param name="id">The identifier for the application instance.</param>
		void UnregisterInstance(Guid id);
	}
}
