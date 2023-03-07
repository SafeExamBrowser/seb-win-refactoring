/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Client.Operations
{
	internal class ApplicationOperation : ClientOperation
	{
		private IApplicationFactory factory;
		private ILogger logger;
		private IApplicationMonitor monitor;
		private IText text;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ApplicationOperation(
			ClientContext context,
			IApplicationFactory factory,
			IApplicationMonitor monitor,
			ILogger logger,
			IText text) : base(context)
		{
			this.factory = factory;
			this.monitor = monitor;
			this.logger = logger;
			this.text = text;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing applications...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeApplications);

			var result = InitializeApplications();

			if (result == OperationResult.Success)
			{
				StartMonitor();
			}

			return result;
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing applications...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeApplications);

			FinalizeApplications();
			StopMonitor();

			return OperationResult.Success;
		}

		private OperationResult InitializeApplications()
		{
			var initialization = monitor.Initialize(Context.Settings.Applications);
			var result = OperationResult.Success;

			if (initialization.FailedAutoTerminations.Any())
			{
				result = HandleAutoTerminationFailure(initialization.FailedAutoTerminations);
			}
			else if (initialization.RunningApplications.Any())
			{
				result = TryTerminate(initialization.RunningApplications);
			}

			if (result == OperationResult.Success)
			{
				foreach (var application in Context.Settings.Applications.Whitelist)
				{
					Initialize(application);
				}
			}

			return result;
		}

		private void Initialize(WhitelistApplication settings)
		{
			var result = factory.TryCreate(settings, out var application);

			while (result == FactoryResult.NotFound && settings.AllowCustomPath)
			{
				var args = new ApplicationNotFoundEventArgs(settings.DisplayName, settings.ExecutableName);

				ActionRequired?.Invoke(args);

				if (args.Success)
				{
					settings.ExecutablePath = args.CustomPath;
					result = factory.TryCreate(settings, out application);
				}
				else
				{
					break;
				}
			}

			if (result == FactoryResult.Success)
			{
				Context.Applications.Add(application);
			}
			else
			{
				logger.Error($"Failed to initialize application '{settings.DisplayName}' ({settings.ExecutableName}). Reason: {result}.");
				ActionRequired?.Invoke(new ApplicationInitializationFailedEventArgs(settings.DisplayName, settings.ExecutableName, result));
			}
		}

		private void FinalizeApplications()
		{
			foreach (var application in Context.Applications)
			{
				application.Terminate();
			}
		}

		private OperationResult HandleAutoTerminationFailure(IList<RunningApplication> applications)
		{
			logger.Error($"{applications.Count} application(s) could not be automatically terminated: {string.Join(", ", applications.Select(a => a.Name))}");
			ActionRequired?.Invoke(new ApplicationTerminationFailedEventArgs(applications));

			return OperationResult.Failed;
		}

		private void StartMonitor()
		{
			if (Context.Settings.Security.KioskMode != KioskMode.None)
			{
				monitor.Start();
			}
		}

		private void StopMonitor()
		{
			if (Context.Settings.Security.KioskMode != KioskMode.None)
			{
				monitor.Stop();
			}
		}

		private OperationResult TryTerminate(IEnumerable<RunningApplication> runningApplications)
		{
			var args = new ApplicationTerminationEventArgs(runningApplications);
			var failed = new List<RunningApplication>();
			var result = OperationResult.Success;

			logger.Info($"The following applications need to be terminated: {string.Join(", ", runningApplications.Select(a => a.Name))}.");
			ActionRequired?.Invoke(args);

			if (args.TerminateProcesses)
			{
				logger.Info($"The user chose to automatically terminate all running applications.");

				foreach (var application in runningApplications)
				{
					var success = monitor.TryTerminate(application);

					if (success)
					{
						logger.Info($"Successfully terminated application '{application.Name}'.");
					}
					else
					{
						result = OperationResult.Failed;
						failed.Add(application);
						logger.Error($"Failed to automatically terminate application '{application.Name}'!");
					}
				}
			}
			else
			{
				logger.Info("The user chose not to automatically terminate all running applications. Aborting...");
				result = OperationResult.Aborted;
			}

			if (failed.Any())
			{
				ActionRequired?.Invoke(new ApplicationTerminationFailedEventArgs(failed));
			}

			return result;
		}
	}
}
