/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Timers;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplicationInstance : IApplicationInstance
	{
		private const int ONE_SECOND = 1000;

		private ILogger logger;
		private string name;
		private IProcess process;
		private Timer timer;

		public IconResource Icon { get; }
		public InstanceIdentifier Id { get; }
		public string Name { get; }

		public event IconChangedEventHandler IconChanged { add { } remove { } }
		public event NameChangedEventHandler NameChanged;
		public event InstanceTerminatedEventHandler Terminated;

		public ExternalApplicationInstance(IconResource icon, InstanceIdentifier id, ILogger logger, IProcess process)
		{
			this.Icon = icon;
			this.Id = id;
			this.logger = logger;
			this.process = process;
		}

		public void Activate()
		{
			var success = process.TryActivate();

			if (!success)
			{
				logger.Warn("Failed to activate instance!");
			}
		}

		public void Initialize()
		{
			InitializeEvents();
			logger.Info("Initialized application instance.");
		}

		public void Terminate()
		{
			const int MAX_ATTEMPTS = 5;
			const int TIMEOUT_MS = 500;

			var terminated = process.HasTerminated;

			if (!terminated)
			{
				FinalizeEvents();

				for (var attempt = 0; attempt < MAX_ATTEMPTS && !terminated; attempt++)
				{
					terminated = process.TryClose(TIMEOUT_MS);
				}

				for (var attempt = 0; attempt < MAX_ATTEMPTS && !terminated; attempt++)
				{
					terminated = process.TryKill(TIMEOUT_MS);
				}

				if (terminated)
				{
					logger.Info("Successfully terminated application instance.");
				}
				else
				{
					logger.Warn("Failed to terminate application instance!");
				}
			}
		}

		private void Process_Terminated(int exitCode)
		{
			logger.Info($"Application instance has terminated with exit code {exitCode}.");
			FinalizeEvents();
			Terminated?.Invoke(Id);
		}

		private void InitializeEvents()
		{
			timer = new Timer(ONE_SECOND);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
			process.Terminated += Process_Terminated;
		}

		private void FinalizeEvents()
		{
			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();
			process.Terminated -= Process_Terminated;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var success = process.TryGetWindowTitle(out var title);
			var hasChanged = name?.Equals(title, StringComparison.Ordinal) != true;

			if (success && hasChanged)
			{
				name = title;
				NameChanged?.Invoke(name);
			}

			timer.Start();
		}
	}
}
