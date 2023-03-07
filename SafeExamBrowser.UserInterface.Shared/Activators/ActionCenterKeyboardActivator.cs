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
	public class ActionCenterKeyboardActivator : KeyboardActivator, IActionCenterActivator
	{
		private readonly ILogger logger;
		private bool A, LeftWindows;

		public event ActivatorEventHandler Activated { add { } remove { } }
		public event ActivatorEventHandler Deactivated { add { } remove { } }
		public event ActivatorEventHandler Toggled;

		public ActionCenterKeyboardActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
		}

		protected override void OnBeforeResume()
		{
			A = false;
			LeftWindows = false;
		}

		protected override bool Process(Key key, KeyModifier modifier, KeyState state)
		{
			var changed = false;
			var pressed = state == KeyState.Pressed;

			switch (key)
			{
				case Key.A:
					changed = A != pressed;
					A = pressed;
					break;
				case Key.LWin:
					changed = LeftWindows != pressed;
					LeftWindows = pressed;
					break;
			}

			if (A && LeftWindows && changed)
			{
				logger.Debug("Detected toggle sequence for action center.");
				Toggled?.Invoke();
			}

			return false;
		}
	}
}
