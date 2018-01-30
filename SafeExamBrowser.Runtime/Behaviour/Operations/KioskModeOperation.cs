/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
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

		public bool AbortStartup { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public KioskModeOperation(ILogger logger, ISettingsRepository settingsRepository)
		{
			this.logger = logger;
			this.settingsRepository = settingsRepository;
		}

		public void Perform()
		{
			kioskMode = settingsRepository.Current.KioskMode;

			logger.Info($"Initializing kiosk mode '{kioskMode}'...");
			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeKioskMode);

			if (kioskMode == KioskMode.CreateNewDesktop)
			{
				CreateNewDesktop();
			}
			else
			{
				DisableExplorerShell();
			}
		}

		public void Revert()
		{
			logger.Info($"Reverting kiosk mode '{kioskMode}'...");
			SplashScreen.UpdateText(TextKey.SplashScreen_RevertKioskMode);

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
			
		}

		private void CloseNewDesktop()
		{
			
		}

		private void DisableExplorerShell()
		{
			
		}

		private void RestartExplorerShell()
		{
			
		}
	}
}
