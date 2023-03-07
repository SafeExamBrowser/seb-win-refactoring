/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Registry;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionIntegrityOperation : SessionOperation
	{
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

			return VerifyEaseOfAccessConfiguration();
		}

		public override OperationResult Repeat()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_VerifySessionIntegrity);

			return VerifyEaseOfAccessConfiguration();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult VerifyEaseOfAccessConfiguration()
		{
			var result = OperationResult.Failed;

			logger.Info($"Attempting to verify ease of access configuration...");

			if (registry.TryRead(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name, out var value))
			{
				if (value == default || (value is string s && string.IsNullOrWhiteSpace(s)))
				{
					result = OperationResult.Success;
					logger.Info("Ease of access configuration successfully verified.");
				}
				else if (!Context.Next.Settings.Service.IgnoreService)
				{
					result = OperationResult.Success;
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

			return result;
		}
	}
}
