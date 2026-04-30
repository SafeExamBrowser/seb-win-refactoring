/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class IntegrityResponsibility : ClientResponsibility
	{
		private readonly ICoordinator coordinator;
		private readonly IText text;
		private readonly DispatcherTimer timer;

		private IIntegrityModule IntegrityModule => Context.IntegrityModule;

		public IntegrityResponsibility(ClientContext context, ICoordinator coordinator, ILogger logger, IText text) : base(context, logger)
		{
			this.coordinator = coordinator;
			this.text = text;
			this.timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.PrepareShutdown_Wave2:
					StopIntegrityMonitoring();
					break;
				case ClientTask.ScheduleIntegrityVerification:
					ScheduleIntegrityVerification();
					break;
				case ClientTask.StartMonitoring:
					StartIntegrityMonitoring();
					break;
				case ClientTask.UpdateSessionIntegrity:
					UpdateSessionIntegrity();
					break;
				case ClientTask.VerifySessionIntegrity:
					VerifySessionIntegrity();
					break;
			}
		}

		private void ScheduleIntegrityVerification()
		{
			var delay = TimeSpan.FromMinutes(10) + TimeSpan.FromMinutes(new Random().NextDouble() * 5);

			Task.Delay(delay).ContinueWith(_ => VerifyApplicationIntegrity());
		}

		private void StartIntegrityMonitoring()
		{
			timer.Interval = TimeSpan.FromSeconds(5);
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void StopIntegrityMonitoring()
		{
			timer.Stop();
			timer.Tick -= Timer_Tick;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			Logger.Info("Attempting to verify runtime integrity...");

			if (IntegrityModule.TryVerifyRuntimeIntegrity(out var isValid))
			{
				HandleRuntimeIntegrityStatus(isValid);
			}
			else
			{
				Logger.Warn("Failed to verify runtime integrity!");
			}
		}

		private void UpdateSessionIntegrity()
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings?.Security.QuitPasswordHash);

			if (hasQuitPassword)
			{
				IntegrityModule?.ClearSession(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl);
			}
		}

		private void VerifyApplicationIntegrity()
		{
			Logger.Info($"Attempting to verify application integrity...");

			if (IntegrityModule.TryVerifyCodeSignature(out var isValid))
			{
				HandleApplicationIntegrityStatus(isValid);
			}
			else
			{
				Logger.Warn("Failed to verify application integrity!");
			}
		}

		private void VerifySessionIntegrity()
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings.Security.QuitPasswordHash);

			if (hasQuitPassword && Settings.Security.VerifySessionIntegrity)
			{
				Logger.Info($"Attempting to verify session integrity...");

				if (IntegrityModule.TryVerifySessionIntegrity(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl, out var isValid))
				{
					HandleSessionIntegrityStatus(isValid);
				}
				else
				{
					Logger.Warn("Failed to verify session integrity!");
				}
			}
		}

		private void HandleApplicationIntegrityStatus(bool isValid)
		{
			if (isValid)
			{
				Logger.Info("Application integrity successfully verified.");
			}
			else if (coordinator.RequestSessionLock())
			{
				Logger.Warn("Application integrity is compromised!");

				ShowLockScreen(text.Get(TextKey.LockScreen_ApplicationIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
				coordinator.ReleaseSessionLock();
			}
			else
			{
				Logger.Warn("Application integrity is compromised but lock screen is already active!");
				Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => VerifyApplicationIntegrity());
			}
		}

		private void HandleRuntimeIntegrityStatus(bool isValid)
		{
			if (isValid)
			{
				Logger.Info("Runtime integrity successfully verified.");
			}
			else if (coordinator.RequestSessionLock())
			{
				Logger.Warn("Runtime integrity is compromised!");

				Task.Run(() =>
				{
					ShowLockScreen(text.Get(TextKey.LockScreen_RuntimeIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
					coordinator.ReleaseSessionLock();
				});
			}
			else
			{
				Logger.Warn("Runtime integrity is compromised but lock screen is already active!");
			}
		}

		private void HandleSessionIntegrityStatus(bool isValid)
		{
			if (isValid)
			{
				Logger.Info("Session integrity successfully verified.");
				IntegrityModule.CacheSession(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl);
			}
			else if (coordinator.RequestSessionLock())
			{
				Logger.Warn("Session integrity is compromised!");

				Task.Delay(1000).ContinueWith(_ =>
				{
					ShowLockScreen(text.Get(TextKey.LockScreen_SessionIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
					coordinator.ReleaseSessionLock();
				});
			}
			else
			{
				Logger.Warn("Session integrity is compromised but lock screen is already active!");
				Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => VerifySessionIntegrity());
			}
		}
	}
}
