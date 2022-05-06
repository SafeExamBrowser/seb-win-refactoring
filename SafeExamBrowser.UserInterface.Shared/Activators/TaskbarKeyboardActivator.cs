/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Shared.Activators
{
	public class TaskbarKeyboardActivator : KeyboardActivator, ITaskbarActivator
	{
		private readonly ILogger logger;
		private bool leftWindows;

		public event ActivatorEventHandler Activated;

		public TaskbarKeyboardActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
		}

		protected override bool Process(Key key, KeyModifier modifier, KeyState state)
		{
			var changed = false;
			var pressed = state == KeyState.Pressed;

			if (key == Key.LWin)
			{
				changed = leftWindows != pressed;
				leftWindows = pressed;
			}

			if (leftWindows && changed)
			{
				logger.Debug("Detected activation sequence for taskbar.");
				Activated?.Invoke();

				return true;
			}

			return false;
		}
	}
}
