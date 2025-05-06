/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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
			var result = FactoryResult.Error;

			application = default;

			try
			{
				var found = TryFindApplication(settings, out var executablePath);
				var valid = found && VerifyApplication(executablePath, name, settings);

				if (found && valid)
				{
					application = InitializeApplication(executablePath, settings);
				}

				result = DetermineResult(name, found, valid);
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to initialize application {name}!", e);
			}

			return result;
		}

		private FactoryResult DetermineResult(string name, bool found, bool valid)
		{
			var result = default(FactoryResult);

			if (!found)
			{
				result = FactoryResult.NotFound;
				logger.Error($"Could not find application {name}!");
			}
			else if (!valid)
			{
				result = FactoryResult.Invalid;
				logger.Error($"The application {name} is not valid or has been manipulated!");
			}
			else
			{
				result = FactoryResult.Success;
				logger.Debug($"Successfully initialized application {name}.");
			}

			return result;
		}

		private IApplication<IApplicationWindow> InitializeApplication(string executablePath, WhitelistApplication settings)
		{
			const int ONE_SECOND = 1000;

			var applicationLogger = logger.CloneFor(settings.DisplayName);
			var application = new ExternalApplication(applicationMonitor, executablePath, applicationLogger, nativeMethods, processFactory, settings, ONE_SECOND);

			application.Initialize();

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

		private bool VerifyApplication(string executablePath, string name, WhitelistApplication settings)
		{
			var valid = true;

			valid &= VerifyName(executablePath, name, settings);
			valid &= VerifyOriginalName(executablePath, name, settings);
			valid &= VerifySignature(executablePath, name, settings);

			return valid;
		}

		private bool VerifyName(string executablePath, string name, WhitelistApplication settings)
		{
			var valid = Path.GetFileName(executablePath).Equals(settings.ExecutableName, StringComparison.OrdinalIgnoreCase);

			if (!valid)
			{
				logger.Warn($"The executable name of application {name} at '{executablePath}' does not match the configured value!");
			}

			return valid;
		}

		private bool VerifyOriginalName(string executablePath, string name, WhitelistApplication settings)
		{
			var ignoreOriginalName = string.IsNullOrWhiteSpace(settings.OriginalName);
			var valid = ignoreOriginalName;

			if (!ignoreOriginalName && TryLoadOriginalName(executablePath, out var originalName))
			{
				valid = originalName.Equals(settings.OriginalName, StringComparison.OrdinalIgnoreCase);
			}

			if (!valid)
			{
				logger.Warn($"The original name of application {name} at '{executablePath}' does not match the configured value!");
			}

			return valid;
		}

		private bool VerifySignature(string executablePath, string name, WhitelistApplication settings)
		{
			var ignoreSignature = string.IsNullOrWhiteSpace(settings.Signature);
			var valid = ignoreSignature;

			if (!ignoreSignature && TryLoadSignature(executablePath, out var signature))
			{
				valid = signature.Equals(settings.Signature, StringComparison.OrdinalIgnoreCase);
			}

			if (!valid)
			{
				logger.Warn($"The signature of application {name} at '{executablePath}' does not match the configured value!");
			}

			return valid;
		}

		private bool TryLoadOriginalName(string path, out string originalName)
		{
			originalName = default;

			try
			{
				originalName = FileVersionInfo.GetVersionInfo(path).OriginalFilename;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to load original name for '{path}'!", e);
			}

			return originalName != default;
		}

		private bool TryLoadSignature(string path, out string signature)
		{
			signature = default;

			try
			{
				using (var certificate = X509Certificate.CreateFromSignedFile(path))
				{
					signature = certificate.GetCertHashString();
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to load signature for '{path}'!", e);
			}

			return signature != default;
		}
	}
}
