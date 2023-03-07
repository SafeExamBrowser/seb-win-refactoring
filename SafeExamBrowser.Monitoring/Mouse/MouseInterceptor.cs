/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Mouse;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Monitoring.Mouse
{
	public class MouseInterceptor : IMouseInterceptor
	{
		private Guid? hookId;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private MouseSettings settings;

		public MouseInterceptor(ILogger logger, INativeMethods nativeMethods, MouseSettings settings)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.settings = settings;
		}

		public void Start()
		{
			hookId = nativeMethods.RegisterMouseHook(MouseHookCallback);
		}

		public void Stop()
		{
			if (hookId.HasValue)
			{
				nativeMethods.DeregisterMouseHook(hookId.Value);
			}
		}

		private bool MouseHookCallback(MouseButton button, MouseButtonState state, MouseInformation info)
		{
			var block = false;

			block |= button == MouseButton.Auxiliary;
			block |= button == MouseButton.Middle && !settings.AllowMiddleButton;
			block |= button == MouseButton.Right && !settings.AllowRightButton;

			if (block)
			{
				logger.Info($"Blocked {button.ToString().ToLower()} mouse button when {state.ToString().ToLower()}.");
			}

			return block;
		}
	}
}
