/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

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
		/// Registers a new instance of an application, to be displayed when the user clicks the taskbar button.
		/// </summary>
		/// <param name="id">The identifier for the application instance.</param>
		/// <param name="title">An optional title to be displayed (if multiple instances are active).</param>
		void RegisterInstance(Guid id, string title = null);

		/// <summary>
		/// Unregisters an application instance, e.g. if it gets closed.
		/// </summary>
		/// <param name="id">The identifier for the application instance.</param>
		void UnregisterInstance(Guid id);
	}
}
