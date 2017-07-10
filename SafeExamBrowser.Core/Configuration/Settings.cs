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
		public string CopyrightInfo
		{
			get
			{
				var executable = Assembly.GetEntryAssembly();
				var copyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

				return copyright;
			}
		}

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
				var newline = Environment.NewLine;
				var executable = Assembly.GetEntryAssembly();
				var title = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;

				var titleLine = $"/* {title}, Version {ProgramVersion}{newline}";
				var copyrightLine = $"/* {CopyrightInfo}{newline}";
				var emptyLine = $"/* {newline}";
				var githubLine = $"/* Please visit https://github.com/SafeExamBrowser for more information.";

				return $"{titleLine}{copyrightLine}{emptyLine}{githubLine}";
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
