/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Lockdown
{
	/// <summary>
	/// Allows to control a feature of the computer, the operating system or an installed third-party software.
	/// </summary>
	public interface IFeatureConfiguration
	{
		/// <summary>
		/// Disables the feature.
		/// </summary>
		void DisableFeature();

		/// <summary>
		/// Enables the feature.
		/// </summary>
		void EnableFeature();

		/// <summary>
		/// Starts monitoring the feature to ensure that it remains as currently configured.
		/// </summary>
		void Monitor();

		/// <summary>
		/// Restores the feature to its initial configuration.
		/// </summary>
		void Restore();
	}
}
