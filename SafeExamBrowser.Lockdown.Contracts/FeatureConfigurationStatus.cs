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
	/// Defines all possible states of an <see cref="IFeatureConfiguration"/>.
	/// </summary>
	public enum FeatureConfigurationStatus
	{
		/// <summary>
		/// The configuration is in an undefined state.
		/// </summary>
		Undefined,

		/// <summary>
		/// The configuration is disabled.
		/// </summary>
		Disabled,

		/// <summary>
		/// The configuration is enabled.
		/// </summary>
		Enabled
	}
}
