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
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class KioskModeOperation : IOperation
	{
		private IConfigurationRepository configuration;
		private IDesktopFactory desktopFactory;
		private KioskMode kioskMode;
		private ILogger logger;
		private IProcessFactory processFactory;
		private IDesktop newDesktop;
		private IDesktop originalDesktop;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public KioskModeOperation(
			IConfigurationRepository configuration,
			IDesktopFactory desktopFactory,
			ILogger logger,
			IProcessFactory processFactory)
		{
			this.configuration = configuration;
			this.desktopFactory = desktopFactory;
			this.logger = logger;
			this.processFactory = processFactory;
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
			originalDesktop = desktopFactory.GetCurrent();
			logger.Info($"Current desktop is {ToString(originalDesktop)}.");
			newDesktop = desktopFactory.CreateNew(nameof(SafeExamBrowser));
			logger.Info($"Created new desktop {ToString(newDesktop)}.");
			newDesktop.Activate();
			logger.Info("Successfully activated new desktop.");
			processFactory.StartupDesktop = newDesktop;
		}

		private void CloseNewDesktop()
		{
			if (originalDesktop != null)
			{
				originalDesktop.Activate();
				processFactory.StartupDesktop = originalDesktop;
				logger.Info($"Switched back to original desktop {ToString(originalDesktop)}.");
			}
			else
			{
				logger.Warn($"No original desktop found when attempting to revert kiosk mode '{kioskMode}'!");
			}

			if (newDesktop != null)
			{
				newDesktop.Close();
				logger.Info($"Closed new desktop {ToString(newDesktop)}.");
			}
			else
			{
				logger.Warn($"No new desktop found when attempting to revert kiosk mode '{kioskMode}'!");
			}
		}

		private void DisableExplorerShell()
		{
			// TODO
		}

		private void RestartExplorerShell()
		{
			// TODO
		}

		private string ToString(IDesktop desktop)
		{
			return $"'{desktop.Name}' [{desktop.Handle}]";
		}
	}
}
