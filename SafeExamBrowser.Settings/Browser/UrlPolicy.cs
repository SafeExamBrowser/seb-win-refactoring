/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all policies for handling of URLs in the user interface and log.
	/// </summary>
	public enum UrlPolicy
	{
		/// <summary>
		/// Always show the URL of a resource instead of the title. Log URLs normally.
		/// </summary>
		Always,

		/// <summary>
		/// Show the URL until the title of a resource is available. Log URLs normally.
		/// </summary>
		BeforeTitle,

		/// <summary>
		/// Only show the URL on load errors, otherwise show the title of a resource. Only log URLs in error messages.
		/// </summary>
		LoadError,

		/// <summary>
		/// Never show the URL of a resource and do not log any URLs.
		/// </summary>
		Never
	}

	public static class UrlPolicyExtensions
	{
		/// <summary>
		/// Indicates whether URLs may be logged normally.
		/// </summary>
		public static bool CanLog(this UrlPolicy policy)
		{
			return policy == UrlPolicy.Always || policy == UrlPolicy.BeforeTitle;
		}

		/// <summary>
		/// Indicates whether URLs may be logged in case of an error.
		/// </summary>
		public static bool CanLogError(this UrlPolicy policy)
		{
			return policy.CanLog() || policy == UrlPolicy.LoadError;
		}
	}
}
