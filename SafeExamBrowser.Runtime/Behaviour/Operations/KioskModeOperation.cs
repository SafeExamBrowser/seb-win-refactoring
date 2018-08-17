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
		private IExplorerShell explorerShell;
		private KioskMode kioskMode;
		private ILogger logger;
		private IProcessFactory processFactory;
		private IDesktop newDesktop;
		private IDesktop originalDesktop;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public KioskModeOperation(
			IConfigurationRepository configuration,
			IDesktopFactory desktopFactory,
			IExplorerShell explorerShell,
			ILogger logger,
			IProcessFactory processFactory)
		{
			this.configuration = configuration;
			this.desktopFactory = desktopFactory;
			this.explorerShell = explorerShell;
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
					TerminateExplorerShell();
					break;
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			var oldMode = kioskMode;
			var newMode = configuration.CurrentSettings.KioskMode;

			if (newMode == oldMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is equal to the currently active '{oldMode}', skipping re-initialization...");
			}
			else
			{
				Revert();
				Perform();
			}

			kioskMode = newMode;

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
			logger.Info($"Current desktop is {originalDesktop}.");

			newDesktop = desktopFactory.CreateNew(nameof(SafeExamBrowser));
			logger.Info($"Created new desktop {newDesktop}.");

			newDesktop.Activate();
			processFactory.StartupDesktop = newDesktop;
			logger.Info("Successfully activated new desktop.");
		}

		private void CloseNewDesktop()
		{
			if (originalDesktop != null)
			{
				originalDesktop.Activate();
				processFactory.StartupDesktop = originalDesktop;
				logger.Info($"Switched back to original desktop {originalDesktop}.");
			}
			else
			{
				logger.Warn($"No original desktop found when attempting to close new desktop!");
			}

			if (newDesktop != null)
			{
				newDesktop.Close();
				logger.Info($"Closed new desktop {newDesktop}.");
			}
			else
			{
				logger.Warn($"No new desktop found when attempting to close new desktop!");
			}
		}

		private void TerminateExplorerShell()
		{
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_WaitExplorerTermination, true);
			explorerShell.Terminate();
		}

		private void RestartExplorerShell()
		{
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_WaitExplorerStartup, true);
			explorerShell.Start();
		}
	}
}
