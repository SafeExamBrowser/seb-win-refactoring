/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Applications.Events;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplicationInstance
	{
		private readonly object @lock = new object();

		private IconResource icon;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private IProcess process;
		private Timer timer;
		private IList<ExternalApplicationWindow> windows;

		internal int Id { get; private set; }

		internal event InstanceTerminatedEventHandler Terminated;
		internal event WindowsChangedEventHandler WindowsChanged;

		internal ExternalApplicationInstance(IconResource icon, ILogger logger, INativeMethods nativeMethods, IProcess process)
		{
			this.icon = icon;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.process = process;
			this.windows = new List<ExternalApplicationWindow>();
		}

		internal IEnumerable<IApplicationWindow> GetWindows()
		{
			lock (@lock)
			{
				return new List<IApplicationWindow>(windows);
			}
		}

		internal void Initialize()
		{
			Id = process.Id;
			InitializeEvents();
			logger.Info("Initialized application instance.");
		}

		internal void Terminate()
		{
			const int MAX_ATTEMPTS = 5;
			const int TIMEOUT_MS = 500;

			var terminated = process.HasTerminated;

			if (terminated)
			{
				logger.Info("Application instance is already terminated.");
			}
			else
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

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var changed = false;
			var openWindows = nativeMethods.GetOpenWindows();

			lock (@lock)
			{
				var closedWindows = windows.Where(w => openWindows.All(ow => ow != w.Handle)).ToList();
				var openedWindows = openWindows.Where(ow => windows.All(w => w.Handle != ow) && BelongsToInstance(ow)).ToList();

				foreach (var window in closedWindows)
				{
					changed = true;
					windows.Remove(window);
				}

				foreach (var window in openedWindows)
				{
					changed = true;
					windows.Add(new ExternalApplicationWindow(icon, nativeMethods, window));
				}

				foreach (var window in windows)
				{
					window.Update();
				}
			}

			if (changed)
			{
				WindowsChanged?.Invoke();
			}

			timer.Start();
		}

		private bool BelongsToInstance(IntPtr window)
		{
			return nativeMethods.GetProcessIdFor(window) == process.Id;
		}

		private void InitializeEvents()
		{
			const int ONE_SECOND = 1000;

			process.Terminated += Process_Terminated;

			timer = new Timer(ONE_SECOND);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
		}

		private void FinalizeEvents()
		{
			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();

			process.Terminated -= Process_Terminated;
		}
	}
}
