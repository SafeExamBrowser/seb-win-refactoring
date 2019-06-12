/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class KioskModeOperation : SessionOperation
	{
		private IDesktop newDesktop;
		private IDesktop originalDesktop;
		private IDesktopFactory desktopFactory;
		private IExplorerShell explorerShell;
		private KioskMode? activeMode;
		private ILogger logger;
		private IProcessFactory processFactory;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public KioskModeOperation(
			IDesktopFactory desktopFactory,
			IExplorerShell explorerShell,
			ILogger logger,
			IProcessFactory processFactory,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.desktopFactory = desktopFactory;
			this.explorerShell = explorerShell;
			this.logger = logger;
			this.processFactory = processFactory;
		}

		public override OperationResult Perform()
		{
			logger.Info($"Initializing kiosk mode '{Context.Next.Settings.KioskMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

			activeMode = Context.Next.Settings.KioskMode;

			switch (Context.Next.Settings.KioskMode)
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

		public override OperationResult Repeat()
		{
			var newMode = Context.Next.Settings.KioskMode;
			var result = OperationResult.Success;

			if (activeMode == newMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is the same as the currently active mode, skipping re-initialization...");
			}
			else
			{
				result = Revert();

				if (result == OperationResult.Success)
				{
					result = Perform();
				}
			}

			return result;
		}

		public override OperationResult Revert()
		{
			logger.Info($"Reverting kiosk mode '{activeMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_RevertKioskMode);

			switch (activeMode)
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
			originalDesktop = desktopFactory.GetCurrent();
			logger.Info($"Current desktop is {originalDesktop}.");

			newDesktop = desktopFactory.CreateNew(nameof(SafeExamBrowser));
			logger.Info($"Created new desktop {newDesktop}.");

			newDesktop.Activate();
			processFactory.StartupDesktop = newDesktop;
			logger.Info("Successfully activated new desktop.");

			explorerShell.Suspend();
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
				logger.Warn($"No original desktop found to activate!");
			}

			if (newDesktop != null)
			{
				newDesktop.Close();
				logger.Info($"Closed new desktop {newDesktop}.");
			}
			else
			{
				logger.Warn($"No new desktop found to close!");
			}

			explorerShell.Resume();
		}

		private void TerminateExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerTermination);

			explorerShell.HideAllWindows();
			explorerShell.Terminate();
		}

		private void RestartExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerStartup);

			explorerShell.Start();
			explorerShell.RestoreAllWindows();
		}
	}
}
