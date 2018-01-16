/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration
{
	/// <remarks>
	/// TODO: Replace with proper implementation once configuration aspects are clear...
	/// </remarks>
	public class Settings : ISettings
	{
		private static readonly string LogFileDate = DateTime.Now.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

		public Settings()
		{
			Browser = new BrowserSettings(this);
			Keyboard = new KeyboardSettings();
			Mouse = new MouseSettings();
			Taskbar = new TaskbarSettings();
		}

		public string AppDataFolderName => nameof(SafeExamBrowser);

		public IBrowserSettings Browser { get; private set; }
		public IKeyboardSettings Keyboard { get; private set; }
		public IMouseSettings Mouse { get; private set; }
		public ITaskbarSettings Taskbar { get; private set; }

		public string ApplicationLogFile
		{
			get { return Path.Combine(LogFolderPath, $"{RuntimeIdentifier}_Application.txt"); }
		}

		public string LogFolderPath
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName, "Logs"); }
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

		public string RuntimeIdentifier => LogFileDate;
	}
}
