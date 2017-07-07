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

		public string LogHeader
		{
			get
			{
				var executable = Assembly.GetEntryAssembly();
				var newline = Environment.NewLine;
				var version = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
				var title = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
				var copyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

				var titleLine = $"/* {title}, Version {version}{newline}";
				var copyrightLine = $"/* {copyright}{newline}";
				var emptyLine = $"/* {newline}";
				var license1 = $"/* The source code of this application is subject to the terms of the Mozilla Public{newline}";
				var license2 = $"/* License, v. 2.0. If a copy of the MPL was not distributed with this software, You{newline}";
				var license3 = $"/* can obtain one at http://mozilla.org/MPL/2.0/.{newline}";
				var github = $"/* For more information or to issue bug reports, see https://github.com/SafeExamBrowser.{newline}";

				return $"{titleLine}{copyrightLine}{emptyLine}{license1}{license2}{license3}{emptyLine}{github}";
			}
		}
	}
}
