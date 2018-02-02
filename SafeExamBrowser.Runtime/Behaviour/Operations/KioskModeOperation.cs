/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class KioskModeOperation : IOperation
	{
		private ILogger logger;
		private ISettingsRepository settingsRepository;
		private KioskMode kioskMode;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public KioskModeOperation(ILogger logger, ISettingsRepository settingsRepository)
		{
			this.logger = logger;
			this.settingsRepository = settingsRepository;
		}

		public void Perform()
		{
			kioskMode = settingsRepository.Current.KioskMode;

			logger.Info($"Initializing kiosk mode '{kioskMode}'...");
			ProgressIndicator?.UpdateText(TextKey.SplashScreen_InitializeKioskMode);

			if (kioskMode == KioskMode.CreateNewDesktop)
			{
				CreateNewDesktop();
			}
			else
			{
				DisableExplorerShell();
			}
		}

		public void Repeat()
		{
			// TODO
		}

		public void Revert()
		{
			logger.Info($"Reverting kiosk mode '{kioskMode}'...");
			ProgressIndicator?.UpdateText(TextKey.SplashScreen_RevertKioskMode);

			if (kioskMode == KioskMode.CreateNewDesktop)
			{
				CloseNewDesktop();
			}
			else
			{
				RestartExplorerShell();
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
