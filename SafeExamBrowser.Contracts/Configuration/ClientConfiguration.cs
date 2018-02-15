/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	[Serializable]
	public class ClientConfiguration
	{
		/// <summary>
		/// The unique identifier for the current session.
		/// </summary>
		public Guid SessionId { get; set; }

		/// <summary>
		/// The application settings to be used by the client.
		/// </summary>
		public Settings.Settings Settings { get; set; }

		/// <summary>
		/// The information about the current runtime.
		/// </summary>
		public RuntimeInfo RuntimeInfo { get; set; }
	}
}
