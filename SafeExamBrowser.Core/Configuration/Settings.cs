/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Core.Configuration
{
	public class Settings : ISettings
	{
		public string LogFolderPath
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SafeExamBrowser", "Logs");
			}
		}
	}
}
