/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using System.Windows;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Shared.Activators
{
	public class ActionCenterTouchActivator : TouchActivator, IActionCenterActivator
	{
		private const int ACTIVATION_INTERVAL = 100;

		private bool isDown;
		private ILogger logger;
		private INativeMethods nativeMethods;

		public event ActivatorEventHandler Activated;
		public event ActivatorEventHandler Deactivated { add { } remove { } }
		public event ActivatorEventHandler Toggled { add { } remove { } }

		public ActionCenterTouchActivator(ILogger logger, INativeMethods nativeMethods) : base(nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		protected override void OnBeforeResume()
		{
			isDown = false;
		}

		protected override bool Process(MouseButton button, MouseButtonState state, MouseInformation info)
		{
			var inActivationArea = 0 < info.X && info.X < SystemParameters.PrimaryScreenWidth * 0.1;

			if (button == MouseButton.Left)
			{
				if (state == MouseButtonState.Released)
				{
					isDown = false;
				}

				if (state == MouseButtonState.Pressed && inActivationArea)
				{
					isDown = true;

					Task.Delay(ACTIVATION_INTERVAL).ContinueWith(async _ =>
					{
						for (var t = 0; t < 4; t++)
						{
							if (WasActivated())
							{
								break;
							}

							await Task.Delay(ACTIVATION_INTERVAL);
						}
					});
				}
			}

			return false;
		}

		private bool WasActivated()
		{
			var (x, y) = nativeMethods.GetCursorPosition();
			var hasMoved = x > SystemParameters.PrimaryScreenWidth * 0.1;
			var activate = isDown && hasMoved;

			if (activate)
			{
				logger.Debug("Detected activation gesture for action center.");
				Activated?.Invoke();
			}

			return activate;
		}
	}
}
