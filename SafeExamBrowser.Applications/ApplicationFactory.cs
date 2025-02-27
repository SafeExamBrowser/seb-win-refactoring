/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	public class ApplicationFactory : IApplicationFactory
	{
		private readonly IApplicationMonitor applicationMonitor;
		private readonly IModuleLogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly IProcessFactory processFactory;
		private readonly IRegistry registry;

		public ApplicationFactory(
			IApplicationMonitor applicationMonitor,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IProcessFactory processFactory,
			IRegistry registry)
		{
			this.applicationMonitor = applicationMonitor;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.processFactory = processFactory;
			this.registry = registry;
		}

		public FactoryResult TryCreate(WhitelistApplication settings, out IApplication<IApplicationWindow> application)
		{
			var name = $"'{settings.DisplayName}' ({settings.ExecutableName})";

			application = default;

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

		private IApplication<IApplicationWindow> BuildApplication(string executablePath, WhitelistApplication settings)
		{
			const int ONE_SECOND = 1000;

			var applicationLogger = logger.CloneFor(settings.DisplayName);
			var application = new ExternalApplication(applicationMonitor, executablePath, applicationLogger, nativeMethods, processFactory, settings, ONE_SECOND);

			return application;
		}

		private bool TryFindApplication(WhitelistApplication settings, out string mainExecutable)
		{
			var paths = new List<string[]>();
			var registryPath = QueryPathFromRegistry(settings);

			mainExecutable = default;

			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.System), settings.ExecutableName });
			paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), settings.ExecutableName });

			if (settings.ExecutablePath != default)
			{
				paths.Add(new[] { settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.System), settings.ExecutablePath, settings.ExecutableName });
				paths.Add(new[] { Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), settings.ExecutablePath, settings.ExecutableName });
			}

			if (registryPath != default)
			{
				paths.Add(new[] { registryPath, settings.ExecutableName });

				if (settings.ExecutablePath != default)
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
			if (registry.TryRead($@"{RegistryValue.MachineHive.AppPaths_Key}\{settings.ExecutableName}", "Path", out var value))
			{
				return value as string;
			}

			return default;
		}
	}
}
