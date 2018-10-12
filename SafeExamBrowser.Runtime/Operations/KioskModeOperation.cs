/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
		protected IDesktopFactory desktopFactory;
		protected IExplorerShell explorerShell;
		protected ILogger logger;
		protected IProcessFactory processFactory;

		private static IDesktop newDesktop;
		private static IDesktop originalDesktop;

		protected static KioskMode? ActiveMode { get; private set; }

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

			switch (Context.Next.Settings.KioskMode)
			{
				case KioskMode.CreateNewDesktop:
					CreateNewDesktop();
					break;
				case KioskMode.DisableExplorerShell:
					TerminateExplorerShell();
					break;
			}

			ActiveMode = Context.Next.Settings.KioskMode;

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			var newMode = Context.Next.Settings.KioskMode;

			if (ActiveMode == newMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is already active, skipping initialization...");

				return OperationResult.Success;
			}

			return Perform();
		}

		public override OperationResult Revert()
		{
			logger.Info($"Reverting kiosk mode '{ActiveMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_RevertKioskMode);

			switch (ActiveMode)
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

			explorerShell.Resume();
		}

		private void TerminateExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerTermination);

			// TODO: Hiding all windows must be done here, as the explorer shell is needed to do so!

			explorerShell.Terminate();
		}

		private void RestartExplorerShell()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitExplorerStartup);
			explorerShell.Start();

			// TODO: Restore all hidden windows!
		}
	}
}
