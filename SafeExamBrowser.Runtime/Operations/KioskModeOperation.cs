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
		private IDesktopFactory desktopFactory;
		private IExplorerShell explorerShell;
		private IProcessFactory processFactory;

		protected ILogger logger;

		private IDesktop NewDesktop
		{
			get { return Context.NewDesktop; }
			set { Context.NewDesktop = value; }
		}

		private IDesktop OriginalDesktop
		{
			get { return Context.OriginalDesktop; }
			set { Context.OriginalDesktop = value; }
		}

		/// <summary>
		/// TODO: This mechanism exposes the internal state of the operation! Find better solution which will keep the
		/// state internal but still allow unit testing of both kiosk mode operations independently!
		/// </summary>
		protected KioskMode? ActiveMode
		{
			get { return Context.ActiveMode; }
			set { Context.ActiveMode = value; }
		}

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
