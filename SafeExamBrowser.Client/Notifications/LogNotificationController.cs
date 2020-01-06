/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Notifications
{
	internal class LogNotificationController : INotificationController
	{
		private ILogger logger;
		private IUserInterfaceFactory uiFactory;
		private IWindow window;

		public LogNotificationController(ILogger logger, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.uiFactory = uiFactory;
		}

		public void Activate()
		{
			if (window == default(IWindow))
			{
				window = uiFactory.CreateLogWindow(logger);

				window.Closing += () => window = null;
				window.Show();
			}
			else
			{
				window.BringToForeground();
			}
		}

		public void Terminate()
		{
			window?.Close();
		}
	}
}
