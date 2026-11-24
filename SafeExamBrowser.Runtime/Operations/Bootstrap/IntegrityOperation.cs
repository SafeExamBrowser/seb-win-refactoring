/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Runtime.Operations.Bootstrap
{
	internal class IntegrityOperation : IOperation
	{
		private readonly IIntegrityModule module;
		private readonly ILogger logger;

		public event StatusChangedEventHandler StatusChanged;

		public IntegrityOperation(IIntegrityModule module, ILogger logger)
		{
			this.module = module;
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			logger.Info("Attempting to verify application and runtime integrity...");
			StatusChanged?.Invoke(TextKey.OperationStatus_VerifyIntegrity);

			VerifyApplicationIntegrity();
			VerifyRuntimeIntegrity();

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void VerifyApplicationIntegrity()
		{
			if (module.TryVerifyApplicationIntegrity(out var isValid))
			{
				if (isValid)
				{
					logger.Info("Application integrity successfully verified.");
				}
				else
				{
					logger.Warn("Application integrity is compromised!");
				}
			}
			else
			{
				logger.Warn("Failed to verify application integrity!");
			}
		}

		private bool VerifyRuntimeIntegrity()
		{
			var valid = false;

			if (module.TryVerifyRuntimeIntegrity(out var violations))
			{
				valid = violations == RuntimeIntegrityViolation.None;

				if (valid)
				{
					logger.Info("Runtime integrity successfully verified.");
				}
				else
				{
					logger.Warn($"Runtime integrity is compromised ({violations:G})!");
				}
			}
			else
			{
				logger.Warn("Failed to verify runtime integrity!");
			}

			return valid;
		}
	}
}
