/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Shared.Activators
{
	public class VerificatorActivator : KeyboardActivator, IVerificatorActivator
	{
		private readonly ILogger logger;

		private bool Ctrl, Shift, UpArrow;

		public event ActivatorEventHandler Activated;

		public VerificatorActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
		}

		protected override void OnBeforeResume()
		{
			Ctrl = false;
			Shift = false;
			UpArrow = false;
		}

		protected override bool Process(Key key, KeyModifier modifier, KeyState state)
		{
			var changed = false;
			var pressed = state == KeyState.Pressed;

			switch (key)
			{
				case Key.LeftCtrl:
				case Key.RightCtrl:
					changed = Ctrl != pressed;
					Ctrl = pressed;
					break;
				case Key.LeftShift:
				case Key.RightShift:
					changed = Shift != pressed;
					Shift = pressed;
					break;
				case Key.Up:
					changed = UpArrow != pressed;
					UpArrow = pressed;
					break;
			}

			if (Ctrl && Shift && UpArrow && changed)
			{
				Task.Run(() =>
				{
					logger.Debug("Detected activation sequence for verificator.");
					Activated?.Invoke();
				});
			}

			return false;
		}
	}
}
