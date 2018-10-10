/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class KioskModeOperation : IRepeatableOperation
	{
		private IConfigurationRepository configuration;
		private IDesktopFactory desktopFactory;
		private IExplorerShell explorerShell;
		private KioskMode kioskMode;
		private ILogger logger;
		private IProcessFactory processFactory;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		private IDesktop NewDesktop
		{
			get { return configuration.CurrentSession.NewDesktop; }
			set { configuration.CurrentSession.NewDesktop = value; }
		}

		private IDesktop OriginalDesktop
		{
			get { return configuration.CurrentSession.OriginalDesktop; }
			set { configuration.CurrentSession.OriginalDesktop = value; }
		}

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

		public virtual OperationResult Perform()
		{
			kioskMode = configuration.CurrentSettings.KioskMode;

			logger.Info($"Initializing kiosk mode '{kioskMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

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

		public virtual OperationResult Repeat()
		{
			var oldMode = kioskMode;
			var newMode = configuration.CurrentSettings.KioskMode;

			if (newMode == oldMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is equal to the currently active '{oldMode}', skipping re-initialization...");

				return OperationResult.Success;
			}

			return Perform();
		}

		public virtual OperationResult Revert()
		{
			logger.Info($"Reverting kiosk mode '{kioskMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_RevertKioskMode);

			switch (kioskMode)
			{
				case KioskMode.CreateNewDesktop:
					CloseNewDesktop();
					break;
				case KioskMode.DisableExplorerShell:
					RestartExplorerShell();
					break;
			}

			return OperationResult.Success;
		}

		private void CreateNewDesktop()
		{
			OriginalDesktop = desktopFactory.GetCurrent();
			logger.Info($"Current desktop is {OriginalDesktop}.");

			NewDesktop = desktopFactory.CreateNew(nameof(SafeExamBrowser));
			logger.Info($"Created new desktop {NewDesktop}.");

			NewDesktop.Activate();
			processFactory.StartupDesktop = NewDesktop;
			logger.Info("Successfully activated new desktop.");

			explorerShell.Suspend();
		}

		private void CloseNewDesktop()
		{
			if (OriginalDesktop != null)
			{
				OriginalDesktop.Activate();
				processFactory.StartupDesktop = OriginalDesktop;
				logger.Info($"Switched back to original desktop {OriginalDesktop}.");
			}
			else
			{
				logger.Warn($"No original desktop found when attempting to close new desktop!");
			}

			if (NewDesktop != null)
			{
				NewDesktop.Close();
				logger.Info($"Closed new desktop {NewDesktop}.");
			}
			else
			{
				logger.Warn($"No new desktop found when attempting to close new desktop!");
			}

			explorerShell.Resume();
		}

		private void TerminateExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerTermination);
			explorerShell.Terminate();
		}

		private void RestartExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerStartup);
			explorerShell.Start();
		}
	}
}
