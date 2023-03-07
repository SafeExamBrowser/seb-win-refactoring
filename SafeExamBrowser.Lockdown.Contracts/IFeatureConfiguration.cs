/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Lockdown.Contracts
{
	/// <summary>
	/// Allows to control a feature of the computer, the operating system or an installed software.
	/// </summary>
	public interface IFeatureConfiguration
	{
		/// <summary>
		/// The unique identifier of this feature configuration.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The identifier of the group of changes to which this feature configuration belongs.
		/// </summary>
		Guid GroupId { get; }

		/// <summary>
		/// Disables the feature. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool DisableFeature();

		/// <summary>
		/// Enables the feature. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool EnableFeature();

		/// <summary>
		/// Retrieves the current status of the configuration.
		/// </summary>
		FeatureConfigurationStatus GetStatus();

		/// <summary>
		/// Initializes the currently active configuration of the feature.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Resets the feature to its default configuration. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool Reset();

		/// <summary>
		/// Restores the feature to its previous configuration (i.e. before it was enabled or disabled). Returns <c>true</c> if successful,
		/// otherwise <c>false</c>.
		/// </summary>
		bool Restore();
	}
}
