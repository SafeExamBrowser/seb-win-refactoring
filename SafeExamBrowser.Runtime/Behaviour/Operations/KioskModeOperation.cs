/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
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

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public KioskModeOperation(ILogger logger, IConfigurationRepository configuration)
		{
			this.logger = logger;
			this.configuration = configuration;
		}

		public void Perform()
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
		}

		public void Repeat()
		{
			// TODO
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
