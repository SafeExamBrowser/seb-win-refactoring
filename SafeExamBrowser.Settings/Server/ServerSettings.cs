/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Server
{
	/// <summary>
	/// Defines all settings for a SEB server.
	/// </summary>
	[Serializable]
	public class ServerSettings
	{
		/// <summary>
		/// The discovery URL for the API of the server.
		/// </summary>
		public string ApiUrl { get; set; }

		/// <summary>
		/// The client name for initial authentication with the server.
		/// </summary>
		public string ClientName { get; set; }

		/// <summary>
		/// The client secret for initial authentication with the server.
		/// </summary>
		public string ClientSecret { get; set; }

		/// <summary>
		/// The identifier of the exam to be started. If present, the exam will be automatically started, i.e. the exam selection will be skipped.
		/// </summary>
		public string ExamId { get; set; }

		/// <summary>
		/// The hash code of the password required to perform a fallback.
		/// </summary>
		public string FallbackPasswordHash { get; set; }

		/// <summary>
		/// The institution to be used for identification with the server.
		/// </summary>
		public string Institution { get; set; }

		/// <summary>
		/// Indicates whether SEB will fallback to the start URL in case no connection could be established with the server.
		/// </summary>
		public bool PerformFallback { get; set; }

		/// <summary>
		/// The time interval in milliseconds to be used for ping requests.
		/// </summary>
		public int PingInterval { get; set; }

		/// <summary>
		/// The number of attempts (e.g. when receiving an invalid server response) before performing a fallback or failing.
		/// </summary>
		public int RequestAttempts { get; set; }

		/// <summary>
		/// The time interval in milliseconds to be waited in between attempts.
		/// </summary>
		public int RequestAttemptInterval { get; set; }

		/// <summary>
		/// The timeout in milliseconds (e.g. to wait for a server response) before performing a fallback or failing.
		/// </summary>
		public int RequestTimeout { get; set; }

		/// <summary>
		/// The URL of the server.
		/// </summary>
		public string ServerUrl { get; set; }
	}
}
