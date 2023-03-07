/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	public class ApplicationFactory : IApplicationFactory
	{
		private IApplicationMonitor applicationMonitor;
		private IModuleLogger logger;
		private INativeMethods nativeMethods;
		private IProcessFactory processFactory;

		public ApplicationFactory(
			IApplicationMonitor applicationMonitor,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IProcessFactory processFactory)
		{
			this.applicationMonitor = applicationMonitor;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.processFactory = processFactory;
		}

		public FactoryResult TryCreate(WhitelistApplication settings, out IApplication application)
		{
			var name = $"'{settings.DisplayName}' ({ settings.ExecutableName})";

			application = default(IApplication);

			try
			{
				var success = TryFindApplication(settings, out var executablePath);

				if (success)
				{
					application = BuildApplication(executablePath, settings);
					application.Initialize();

					logger.Debug($"Successfully initialized application {name}.");

					return FactoryResult.Success;
				}

				logger.Error($"Could not find application {name}!");

				return FactoryResult.NotFound;
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to initialize application {name}!", e);
			}

			return FactoryResult.Error;
		}

		private IApplication BuildApplication(string executablePath, WhitelistApplication settings)
		{
			var applicationLogger = logger.CloneFor(settings.DisplayName);
			var application = new ExternalApplication(applicationMonitor, executablePath, applicationLogger, nativeMethods, processFactory, settings);

			return application;
		}

		private bool TryFindApplication(WhitelistApplication settings, out string mainExecutable)
		{
			var paths = new List<string[]>();
			var registryPath = QueryPathFromRegistry(settings);

			mainExecutable = default(string);

			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.System), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), settings.ExecutableName });

			if (settings.ExecutablePath != default(string))
			{
				paths.Add(new[] { settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.System), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), settings.ExecutablePath, settings.ExecutableName });
			}

			if (registryPath != default(string))
			{
				paths.Add(new[] { registryPath, settings.ExecutableName });

				if (settings.ExecutablePath != default(string))
				{
					paths.Add(new[] { registryPath, settings.ExecutablePath, settings.ExecutableName });
				}
			}

			foreach (var path in paths)
			{
				try
				{
					mainExecutable = Path.Combine(path);
					mainExecutable = Environment.ExpandEnvironmentVariables(mainExecutable);

					if (File.Exists(mainExecutable))
					{
						return true;
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to test path {string.Join(@"\", path)}!", e);
				}
			}

			return false;
		}

		private string QueryPathFromRegistry(WhitelistApplication settings)
		{
			try
			{
				using (var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{settings.ExecutableName}"))
				{
					if (key != null)
					{
						return key.GetValue("Path") as string;
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to query path in registry for '{settings.ExecutableName}'!", e);
			}

			return default(string);
		}
	}
}
