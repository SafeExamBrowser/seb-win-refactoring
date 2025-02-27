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
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class IntegrityResponsibility : ClientResponsibility
	{
		private readonly IText text;

		private IIntegrityModule IntegrityModule => Context.IntegrityModule;

		public IntegrityResponsibility(ClientContext context, ILogger logger, IText text) : base(context, logger)
		{
			this.text = text;
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.ScheduleIntegrityVerification:
					ScheduleIntegrityVerification();
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
			const int FIVE_MINUTES = 300000;
			const int TEN_MINUTES = 600000;

			var timer = new System.Timers.Timer();

			timer.AutoReset = false;
			timer.Elapsed += (o, args) => VerifyApplicationIntegrity();
			timer.Interval = TEN_MINUTES + (new Random().NextDouble() * FIVE_MINUTES);
			timer.Start();
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
				if (isValid)
				{
					Logger.Info("Application integrity successfully verified.");
				}
				else
				{
					Logger.Warn("Application integrity is compromised!");
					ShowLockScreen(text.Get(TextKey.LockScreen_ApplicationIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
				}
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
					if (isValid)
					{
						Logger.Info("Session integrity successfully verified.");
						IntegrityModule.CacheSession(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl);
					}
					else
					{
						Logger.Warn("Session integrity is compromised!");
						Task.Delay(1000).ContinueWith(_ =>
						{
							ShowLockScreen(text.Get(TextKey.LockScreen_SessionIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
						});
					}
				}
				else
				{
					Logger.Warn("Failed to verify session integrity!");
				}
			}
		}
	}
}
