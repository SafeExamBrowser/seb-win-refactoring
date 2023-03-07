/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
	public class TaskviewKeyboardActivator : KeyboardActivator, ITaskviewActivator
	{
		private bool Activated, LeftShift, Tab;
		private ILogger logger;

		public event ActivatorEventHandler Deactivated;
		public event ActivatorEventHandler NextActivated;
		public event ActivatorEventHandler PreviousActivated;

		public TaskviewKeyboardActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
		}

		protected override void OnBeforePause()
		{
			if (Activated)
			{
				logger.Debug("Auto-deactivation.");
				Deactivated?.Invoke();
			}

			Activated = false;
		}

		protected override void OnBeforeResume()
		{
			Activated = false;
			LeftShift = false;
			Tab = false;
		}

		protected override bool Process(Key key, KeyModifier modifier, KeyState state)
		{
			if (IsDeactivation(modifier))
			{
				return false;
			}

			if (IsActivation(key, modifier, state))
			{
				return true;
			}

			return false;
		}

		private bool IsActivation(Key key, KeyModifier modifier, KeyState state)
		{
			var changed = false;
			var pressed = state == KeyState.Pressed && modifier.HasFlag(KeyModifier.Alt);

			switch (key)
			{
				case Key.Tab:
					changed = Tab != pressed;
					Tab = pressed;
					break;
				case Key.LeftShift:
					changed = LeftShift != pressed;
					LeftShift = pressed;
					break;
			}

			var isActivation = Tab && changed;

			if (isActivation)
			{
				Activated = true;

				if (LeftShift)
				{
					logger.Debug("Detected sequence for previous instance.");
					PreviousActivated?.Invoke();
				}
				else
				{
					logger.Debug("Detected sequence for next instance.");
					NextActivated?.Invoke();
				}
			}

			return isActivation;
		}

		private bool IsDeactivation(KeyModifier modifier)
		{
			var isDeactivation = Activated && !modifier.HasFlag(KeyModifier.Alt);

			if (isDeactivation)
			{
				Activated = false;
				LeftShift = false;
				Tab = false;

				logger.Debug("Detected deactivation sequence.");
				Deactivated?.Invoke();
			}

			return isDeactivation;
		}
	}
}
