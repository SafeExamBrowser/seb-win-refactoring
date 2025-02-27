/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;

namespace SafeExamBrowser.Core.Contracts.Notifications
{
	/// <summary>
	/// Controls the lifetime and functionality of a notification which can be activated via the UI.
	/// </summary>
	public interface INotification
	{
		/// <summary>
		/// Determines whether the notification can be activated.
		/// </summary>
		bool CanActivate { get; }

		/// <summary>
		/// The resource providing the notification icon.
		/// </summary>
		IconResource IconResource { get; }

		/// <summary>
		/// The tooltip for the notification.
		/// </summary>
		string Tooltip { get; }

		/// <summary>
		/// Event fired when the notification has changed.
		/// </summary>
		event NotificationChangedEventHandler NotificationChanged;

		/// <summary>
		/// Executes the notification functionality.
		/// </summary>
		void Activate();

		/// <summary>
		/// Terminates the notification functionality and release all used resources.
		/// </summary>
		void Terminate();
	}
}
