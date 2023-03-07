/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Lockdown.Contracts
{
	/// <summary>
	/// Defines the functionality of a backup repository for <see cref="IFeatureConfiguration"/>.
	/// </summary>
	public interface IFeatureConfigurationBackup
	{
		/// <summary>
		/// Deletes the given <see cref="IFeatureConfiguration"/> from the backup repository.
		/// </summary>
		void Delete(IFeatureConfiguration configuration);

		/// <summary>
		/// Gets all <see cref="IFeatureConfiguration"/> currently saved in the backup repository.
		/// </summary>
		IList<IFeatureConfiguration> GetAllConfigurations();

		/// <summary>
		/// Gets all <see cref="IFeatureConfiguration"/> which are part of the given group.
		/// </summary>
		IList<IFeatureConfiguration> GetBy(Guid groupId);

		/// <summary>
		/// Saves the given <see cref="IFeatureConfiguration"/> in the backup repository.
		/// </summary>
		void Save(IFeatureConfiguration configuration);
	}
}
