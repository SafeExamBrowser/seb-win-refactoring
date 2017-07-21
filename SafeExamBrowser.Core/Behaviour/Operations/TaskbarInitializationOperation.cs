/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class TaskbarInitializationOperation : IOperation
	{
		private ILogger logger;
		private ITaskbar taskbar;
		private IUiElementFactory uiFactory;
		private INotificationInfo aboutInfo;

		public ISplashScreen SplashScreen { private get; set; }

		public TaskbarInitializationOperation(ILogger logger, INotificationInfo aboutInfo, ITaskbar taskbar, IUiElementFactory uiFactory)
		{
			this.logger = logger;
			this.aboutInfo = aboutInfo;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public void Perform()
		{
			logger.Info("--- Initializing taskbar ---");
			SplashScreen.UpdateText(Key.SplashScreen_InitializeTaskbar);

			var aboutNotification = uiFactory.CreateNotification(aboutInfo);

			taskbar.AddNotification(aboutNotification);
		}

		public void Revert()
		{
			// Nothing to do here so far...
		}
	}
}
