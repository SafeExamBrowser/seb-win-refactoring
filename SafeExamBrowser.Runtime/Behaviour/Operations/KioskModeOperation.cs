/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class KioskModeOperation : IOperation
	{
		private ILogger logger;
		private IConfigurationRepository configuration;
		private KioskMode kioskMode;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public KioskModeOperation(ILogger logger, IConfigurationRepository configuration)
		{
			this.logger = logger;
			this.configuration = configuration;
		}

		public OperationResult Perform()
		{
			kioskMode = configuration.CurrentSettings.KioskMode;

			logger.Info($"Initializing kiosk mode '{kioskMode}'...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeKioskMode);

			switch (kioskMode)
			{
				case KioskMode.CreateNewDesktop:
					CreateNewDesktop();
					break;
				case KioskMode.DisableExplorerShell:
					DisableExplorerShell();
					break;
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			// TODO: Depends on new kiosk mode!

			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info($"Reverting kiosk mode '{kioskMode}'...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_RevertKioskMode);

			switch (kioskMode)
			{
				case KioskMode.CreateNewDesktop:
					CloseNewDesktop();
					break;
				case KioskMode.DisableExplorerShell:
					RestartExplorerShell();
					break;
			}
		}

		private void CreateNewDesktop()
		{
			// TODO
		}

		private void CloseNewDesktop()
		{
			// TODO
		}

		private void DisableExplorerShell()
		{
			// TODO
		}

		private void RestartExplorerShell()
		{
			// TODO
		}
	}
}
