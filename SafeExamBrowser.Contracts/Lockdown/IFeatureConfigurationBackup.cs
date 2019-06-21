/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.Lockdown
{
	/// <summary>
	/// Defines the functionality of a backup repository for <see cref="IFeatureConfiguration"/>.
	/// </summary>
	public interface IFeatureConfigurationBackup
	{
		/// <summary>
		/// Gets all <see cref="IFeatureConfiguration"/> currently saved in the backup repository.
		/// </summary>
		IList<IFeatureConfiguration> GetConfigurations();

		/// <summary>
		/// Saves the given <see cref="IFeatureConfiguration"/> in the backup repository.
		/// </summary>
		void Save(IFeatureConfiguration configuration);

		/// <summary>
		/// Deletes the given <see cref="IFeatureConfiguration"/> from the backup repository.
		/// </summary>
		void Delete(IFeatureConfiguration configuration);
	}
}
