/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class KioskModeOperation : SessionOperation
	{
		private readonly IDesktopFactory desktopFactory;
		private readonly IDesktopMonitor desktopMonitor;
		private readonly IExplorerShell explorerShell;
		private readonly IProcessFactory processFactory;

		private KioskMode? activeMode;
		private IDesktop customDesktop;
		private IDesktop originalDesktop;

		public override event StatusChangedEventHandler StatusChanged;

		public KioskModeOperation(
			Dependencies dependencies,
			IDesktopFactory desktopFactory,
			IDesktopMonitor desktopMonitor,
			IExplorerShell explorerShell,
			IProcessFactory processFactory) : base(dependencies)
		{
			this.desktopFactory = desktopFactory;
			this.desktopMonitor = desktopMonitor;
			this.explorerShell = explorerShell;
			this.processFactory = processFactory;
		}

		public override OperationResult Perform()
		{
			Logger.Info($"Initializing kiosk mode '{Context.Next.Settings.Security.KioskMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

			activeMode = Context.Next.Settings.Security.KioskMode;

			switch (Context.Next.Settings.Security.KioskMode)
			{
				case KioskMode.CreateNewDesktop:
					CreateCustomDesktop();
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
				Logger.Info($"New kiosk mode '{newMode}' is the same as the currently active mode, skipping re-initialization...");
			}
			else
			{
				Logger.Info($"Switching from kiosk mode '{activeMode}' to '{newMode}'...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeKioskMode);

				switch (activeMode)
				{
					case KioskMode.CreateNewDesktop:
						CloseCustomDesktop();
						break;
					case KioskMode.DisableExplorerShell:
						RestartExplorerShell();
						break;
				}

				activeMode = newMode;

				switch (newMode)
				{
					case KioskMode.CreateNewDesktop:
						CreateCustomDesktop();
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
			Logger.Info($"Reverting kiosk mode '{activeMode}'...");
			StatusChanged?.Invoke(TextKey.OperationStatus_RevertKioskMode);

			switch (activeMode)
			{
				case KioskMode.CreateNewDesktop:
					CloseCustomDesktop();
					break;
				case KioskMode.DisableExplorerShell:
					RestartExplorerShell();
					break;
			}

			return OperationResult.Success;
		}

		private void CreateCustomDesktop()
		{
			originalDesktop = desktopFactory.GetCurrent();
			Logger.Info($"Current desktop is {originalDesktop}.");

			customDesktop = desktopFactory.CreateRandom();
			Logger.Info($"Created custom desktop {customDesktop}.");

			customDesktop.Activate();
			processFactory.StartupDesktop = customDesktop;
			Logger.Info("Successfully activated custom desktop.");

			desktopMonitor.Start(customDesktop);
		}

		private void CloseCustomDesktop()
		{
			desktopMonitor.Stop();

			if (originalDesktop != default)
			{
				originalDesktop.Activate();
				processFactory.StartupDesktop = originalDesktop;
				Logger.Info($"Switched back to original desktop {originalDesktop}.");
			}
			else
			{
				Logger.Warn($"No original desktop found to activate!");
			}

			if (customDesktop != default)
			{
				customDesktop.Close();
				Logger.Info($"Closed custom desktop {customDesktop}.");
			}
			else
			{
				Logger.Warn($"No custom desktop found to close!");
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
