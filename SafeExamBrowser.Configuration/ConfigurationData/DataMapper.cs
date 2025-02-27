/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Configuration.ConfigurationData.DataMapping;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataMapper
	{
		private readonly BaseDataMapper[] mappers =
		{
			new ApplicationDataMapper(),
			new AudioDataMapper(),
			new BrowserDataMapper(),
			new ConfigurationFileDataMapper(),
			new DisplayDataMapper(),
			new GeneralDataMapper(),
			new InputDataMapper(),
			new ProctoringDataMapper(),
			new SecurityDataMapper(),
			new ServerDataMapper(),
			new ServiceDataMapper(),
			new SystemDataMapper(),
			new UserInterfaceDataMapper()
		};

		internal void Map(IDictionary<string, object> rawData, AppSettings settings)
		{
			foreach (var item in rawData)
			{
				foreach (var mapper in mappers)
				{
					mapper.Map(item.Key, item.Value, settings);
				}
			}

			foreach (var mapper in mappers)
			{
				mapper.MapGlobal(rawData, settings);
			}
		}
	}
}
