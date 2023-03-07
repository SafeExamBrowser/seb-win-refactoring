/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Lockdown.Contracts
{
	/// <summary>
	/// Provides functionality to ensure that one or more <see cref="IFeatureConfiguration"/> remain in a given <see cref="FeatureConfigurationStatus"/>.
	/// </summary>
	public interface IFeatureConfigurationMonitor
	{
		/// <summary>
		/// Registers a configuration to be monitored for the given status.
		/// </summary>
		void Observe(IFeatureConfiguration configuration, FeatureConfigurationStatus status);

		/// <summary>
		/// Stops the monitoring activity and removes all observed configurations.
		/// </summary>
		void Reset();

		/// <summary>
		/// Starts the monitoring activity.
		/// </summary>
		void Start();
	}
}
