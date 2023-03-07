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
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class KioskModeOperation : SessionOperation
	{
		private IDesktop newDesktop;
		private IDesktop originalDesktop;
		private IDesktopFactory desktopFactory;
		private IDesktopMonitor desktopMonitor;
		private IExplorerShell explorerShell;
		private KioskMode? activeMode;
		private ILogger logger;
		private IProcessFactory processFactory;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public KioskModeOperation(
			IDesktopFactory desktopFactory,
			IDesktopMonitor desktopMonitor,
			IExplorerShell explorerShell,
			ILogger logger,
			IProcessFactory processFactory,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.desktopFactory = desktopFactory;
			this.desktopMonitor = desktopMonitor;
			this.explorerShell = explorerShell;
			this.logger = logger;
			this.processFactory = processFactory;
		}

		public override OperationResult Perform()
		{
			logger.Info($"Initializing kiosk mode '{Context.Next.Settings.Security.KioskMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

			activeMode = Context.Next.Settings.Security.KioskMode;

			switch (Context.Next.Settings.Security.KioskMode)
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
			var newMode = Context.Next.Settings.Security.KioskMode;

			if (activeMode == newMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is the same as the currently active mode, skipping re-initialization...");
			}
			else
			{
				logger.Info($"Switching from kiosk mode '{activeMode}' to '{newMode}'...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

				switch (activeMode)
				{
					case KioskMode.CreateNewDesktop:
						CloseNewDesktop();
						break;
					case KioskMode.DisableExplorerShell:
						RestartExplorerShell();
						break;
				}

				activeMode = newMode;

				switch (newMode)
				{
					case KioskMode.CreateNewDesktop:
						CreateNewDesktop();
						break;
					case KioskMode.DisableExplorerShell:
						TerminateExplorerShell();
						break;
				}
			}

			return OperationResult.Success;
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

			desktopMonitor.Start(newDesktop);
		}

		private void CloseNewDesktop()
		{
			desktopMonitor.Stop();

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
