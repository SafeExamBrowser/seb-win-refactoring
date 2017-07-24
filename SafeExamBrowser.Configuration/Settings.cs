/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Configuration
{
	public class Settings : ISettings
	{
		private const string AppDataFolder = "SafeExamBrowser";

		public string BrowserCachePath
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolder, "Cache");
			}
		}

		public string LogFolderPath
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolder, "Logs");
			}
		}

		public string ProgramCopyright
		{
			get
			{
				var executable = Assembly.GetEntryAssembly();
				var copyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

				return copyright;
			}
		}

		public string ProgramTitle
		{
			get
			{
				var executable = Assembly.GetEntryAssembly();
				var title = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;

				return title;
			}
		}

		public string ProgramVersion
		{
			get
			{
				var executable = Assembly.GetEntryAssembly();
				var version = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

				return version;
			}
		}
	}
}
