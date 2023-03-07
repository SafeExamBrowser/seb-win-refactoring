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
	public class TerminationActivator : KeyboardActivator, ITerminationActivator
	{
		private bool Q, LeftCtrl, RightCtrl;
		private ILogger logger;

		public event ActivatorEventHandler Activated;

		public TerminationActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
		}

		protected override void OnBeforeResume()
		{
			Q = false;
			LeftCtrl = false;
			RightCtrl = false;
		}

		protected override bool Process(Key key, KeyModifier modifier, KeyState state)
		{
			var changed = false;
			var pressed = state == KeyState.Pressed;

			switch (key)
			{
				case Key.Q:
					changed = Q != pressed;
					Q = pressed;
					break;
				case Key.LeftCtrl:
					changed = LeftCtrl != pressed;
					LeftCtrl = pressed;
					break;
				case Key.RightCtrl:
					changed = RightCtrl != pressed;
					RightCtrl = pressed;
					break;
			}

			if (Q && (LeftCtrl || RightCtrl) && changed && !modifier.HasFlag(KeyModifier.Alt))
			{
				logger.Debug("Detected termination sequence.");
				Activated?.Invoke();

				return true;
			}

			return false;
		}
	}
}
