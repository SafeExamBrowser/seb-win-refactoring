/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapConfigurationMode(Settings settings, object value)
		{
			const int CONFIGURE_CLIENT = 1;

			if (value is int mode)
			{
				settings.ConfigurationMode = mode == CONFIGURE_CLIENT ? ConfigurationMode.ConfigureClient : ConfigurationMode.Exam;
			}
		}
	}
}
