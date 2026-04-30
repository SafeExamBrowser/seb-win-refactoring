/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class IntegrityResponsibility : RuntimeResponsibility
	{
		private readonly IIntegrityModule integrityModule;
		private readonly Action shutdown;
		private readonly DispatcherTimer timer;

		public IntegrityResponsibility(
			IIntegrityModule integrityModule,
			ILogger logger,
			RuntimeContext runtimeContext,
			Action shutdown) : base(logger, runtimeContext)
		{
			this.integrityModule = integrityModule;
			this.shutdown = shutdown;
			this.timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
		}

		public override void Assume(RuntimeTask task)
		{
			switch (task)
			{
				case RuntimeTask.StartIntegrityMonitoring:
					StartIntegrityMonitoring();
					break;
				case RuntimeTask.StopIntegrityMonitoring:
					StopIntegrityMonitoring();
					break;
			}
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

			if (integrityModule.TryVerifyRuntimeIntegrity(out var isValid))
			{
				HandleRuntimeIntegrityStatus(isValid);
			}
			else
			{
				Logger.Warn("Failed to verify runtime integrity!");
			}
		}

		private void HandleRuntimeIntegrityStatus(bool isValid)
		{
			if (isValid)
			{
				Logger.Info("Runtime integrity successfully verified.");
			}
			else
			{
				Logger.Error("Runtime integrity is compromised!");

				Task.Run(() =>
				{
					if (SessionIsRunning)
					{
						Context.Responsibilities.Delegate(RuntimeTask.StopSession);
					}

					shutdown.Invoke();
				});
			}
		}
	}
}
