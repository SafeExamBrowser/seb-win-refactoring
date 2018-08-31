/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	/// <summary>
	/// The button of a (third-party) application which can be loaded into the <see cref="ITaskbar"/>.
	/// </summary>
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
