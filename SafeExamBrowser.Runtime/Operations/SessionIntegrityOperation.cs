/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Registry;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionIntegrityOperation : SessionOperation
	{
		private static readonly string USER_PATH = $@"{Environment.ExpandEnvironmentVariables("%LocalAppData%")}\Microsoft\Windows\Cursors\";
		private static readonly string SYSTEM_PATH = $@"{Environment.ExpandEnvironmentVariables("%SystemRoot%")}\Cursors\";

		private readonly ILogger logger;
		private readonly IRegistry registry;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public SessionIntegrityOperation(ILogger logger, IRegistry registry, SessionContext context) : base(context)
		{
			this.logger = logger;
			this.registry = registry;
		}

		public override OperationResult Perform()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_VerifySessionIntegrity);

			var success = VerifyCursorConfiguration();

			if (success)
			{
				success = VerifyEaseOfAccessConfiguration();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_VerifySessionIntegrity);

			var success = VerifyCursorConfiguration();

			if (success)
			{
				success = VerifyEaseOfAccessConfiguration();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private bool VerifyCursorConfiguration()
		{
			var success = true;

			logger.Info($"Attempting to verify cursor configuration...");

			if (registry.TryGetNames(RegistryValue.UserHive.Cursors_Key, out var cursors))
			{
				foreach (var cursor in cursors.Where(c => !string.IsNullOrWhiteSpace(c)))
				{
					success &= registry.TryRead(RegistryValue.UserHive.Cursors_Key, cursor, out var value);
					success &= value == default || !(value is string) || (value is string path && (string.IsNullOrWhiteSpace(path) || IsValidCursorPath(path)));

					if (!success)
					{
						logger.Warn($"{(value != default ? $"Cursor configuration is compromised: '{value}'" : $"Failed to verify configuration of cursor '{cursor}'")}! Aborting session initialization...");

						break;
					}
				}

				if (success)
				{
					logger.Info("Cursor configuration successfully verified.");
				}
			}
			else
			{
				logger.Error("Failed to verify cursor configuration!");
			}

			return success;
		}

		private bool IsValidCursorPath(string path)
		{
			return path.StartsWith(USER_PATH, StringComparison.OrdinalIgnoreCase) || path.StartsWith(SYSTEM_PATH, StringComparison.OrdinalIgnoreCase);
		}

		private bool VerifyEaseOfAccessConfiguration()
		{
			var success = false;

			logger.Info($"Attempting to verify ease of access configuration...");

			if (registry.TryRead(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name, out var value))
			{
				if (value == default || (value is string s && string.IsNullOrWhiteSpace(s)))
				{
					success = true;
					logger.Info("Ease of access configuration successfully verified.");
				}
				else if (!Context.Next.Settings.Service.IgnoreService)
				{
					success = true;
					logger.Info($"Ease of access configuration is compromised ('{value}'), but service will be active in the next session.");
				}
				else
				{
					logger.Warn($"Ease of access configuration is compromised: '{value}'! Aborting session initialization...");
				}
			}
			else
			{
				logger.Error("Failed to verify ease of access configuration!");
			}

			return success;
		}
	}
}
