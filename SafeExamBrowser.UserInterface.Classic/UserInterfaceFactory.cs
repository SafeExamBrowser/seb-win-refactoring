/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.UserInterface.Classic
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		public IWindow CreateAboutWindow(ISettings settings, IText text)
		{
			throw new NotImplementedException();
		}

		public IApplicationButton CreateApplicationButton(IApplicationInfo info)
		{
			throw new NotImplementedException();
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, IBrowserSettings settings)
		{
			throw new NotImplementedException();
		}

		public IWindow CreateLogWindow(ILogger logger, IText text)
		{
			throw new NotImplementedException();
		}

		public INotificationButton CreateNotification(INotificationInfo info)
		{
			throw new NotImplementedException();
		}

		public ISystemPowerSupplyControl CreatePowerSupplyControl()
		{
			throw new NotImplementedException();
		}

		public ISplashScreen CreateSplashScreen(ISettings settings, IText text)
		{
			throw new NotImplementedException();
		}

		public void Show(string message, string title, MessageBoxAction action = MessageBoxAction.Confirm, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			throw new NotImplementedException();
		}
	}
}
