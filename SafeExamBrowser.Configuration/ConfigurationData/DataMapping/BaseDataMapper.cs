/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal abstract class BaseDataMapper
	{
		internal abstract void Map(string key, object value, AppSettings settings);
		internal virtual void MapGlobal(IDictionary<string, object> rawData, AppSettings settings) { }
	}
}
