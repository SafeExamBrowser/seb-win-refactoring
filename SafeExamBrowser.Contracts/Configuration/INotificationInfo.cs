/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// The information about a notification which is part of the <see cref="UserInterface.Taskbar.ITaskbar"/>.
	/// </summary>
	public interface INotificationInfo
	{
		/// <summary>
		/// The tooltip for the notification.
		/// </summary>
		string Tooltip { get; }

		/// <summary>
		/// The resource providing the notification icon.
		/// </summary>
		IIconResource IconResource { get; }
	}
}
