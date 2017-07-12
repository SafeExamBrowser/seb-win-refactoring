/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.UserInterface
{
	public interface IUiElementFactory
	{
		/// <summary>
		/// Creates a taskbar button, initialized with the given application information.
		/// </summary>
		ITaskbarButton CreateButton(IApplicationInfo info);
	}
}
