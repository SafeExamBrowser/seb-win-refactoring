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
	/// Defines a mechanism which tries to restore all changes saved in a <see cref="IFeatureConfigurationBackup"/>.
	/// </summary>
	public interface IAutoRestoreMechanism
	{
		/// <summary>
		/// Starts the procedure.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the procedure.
		/// </summary>
		void Stop();
	}
}
