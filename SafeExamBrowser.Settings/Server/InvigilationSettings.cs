/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Server
{
	/// <summary>
	/// Defines all invigilation settings for a server session.
	/// </summary>
	[Serializable]
	public class InvigilationSettings
	{
		/// <summary>
		/// Determines whether the message input for the raise hand notification will be forced.
		/// </summary>
		public bool ForceRaiseHandMessage { get; set; }

		/// <summary>
		/// Determines whether the raise hand notification will be shown in the shell.
		/// </summary>
		public bool ShowRaiseHandNotification { get; set; }
	}
}
