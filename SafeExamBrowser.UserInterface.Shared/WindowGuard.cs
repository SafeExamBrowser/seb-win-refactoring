/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Shared
{
	public class WindowGuard : IWindowGuard
	{
		private readonly ConcurrentStack<Window> cache;
		private readonly ILogger logger;

		private bool? active;

		public WindowGuard(ILogger logger)
		{
			this.cache = new ConcurrentStack<Window>();
			this.logger = logger;
		}

		public void Activate()
		{
			active = true;

			while (cache.Any())
			{
				if (cache.TryPop(out var window))
				{
					Guard(window);
				}
			}
		}

		public void Deactivate()
		{
			active = false;
			cache.Clear();
		}

		public void Register(object @object)
		{
			if (@object is Window window)
			{
				window.ExecuteWithAccess(() => window.Loaded += (o, a) => Triage(window));
			}
			else
			{
				throw new ArgumentException($"The given object '{@object}' is not an application window!", nameof(@object));
			}
		}

		private void Guard(Window window)
		{
			window.ExecuteWithAccess(() =>
			{
				var info = $"{window.GetType().Name}";

				try
				{
					var success = window.ExcludeFromCapture();

					if (success)
					{
						logger.Debug($"Successfully guarded window '{info}'.");
					}
					else
					{
						logger.Warn($"Failed to guard window '{info}'!");
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to guard window '{info}'!", e);
				}
			});
		}

		private void Triage(Window window)
		{
			if (active == true)
			{
				Guard(window);
			}
			else if (!active.HasValue)
			{
				cache.Push(window);
			}
		}
	}
}
