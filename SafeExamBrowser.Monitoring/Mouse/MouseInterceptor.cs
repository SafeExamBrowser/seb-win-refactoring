/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;

namespace SafeExamBrowser.Monitoring.Mouse
{
	public class MouseInterceptor : IMouseInterceptor
	{
		private ILogger logger;
		private MouseSettings settings;

		public MouseInterceptor(ILogger logger, MouseSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public bool Block(MouseButton button, KeyState state)
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
