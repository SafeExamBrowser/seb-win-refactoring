/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
		private IMouseSettings settings;

		public MouseInterceptor(ILogger logger, IMouseSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public bool Block(MouseButton button, KeyState state)
		{
			var block = false;

			block |= !settings.AllowMiddleButton && button == MouseButton.Middle;
			block |= !settings.AllowRightButton && button == MouseButton.Right;

			if (block)
			{
				logger.Info($"Blocked {button.ToString().ToLower()} mouse button when {state.ToString().ToLower()}.");
			}

			return block;
		}
	}
}
