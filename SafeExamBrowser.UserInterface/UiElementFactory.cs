/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Controls;

namespace SafeExamBrowser.UserInterface
{
	public class UiElementFactory : IUiElementFactory
	{
		public ITaskbarButton CreateApplicationButton(IApplicationInfo info)
		{
			return new ApplicationButton(info);
		}

		public ISplashScreen CreateSplashScreen(ISettings settings, IText text)
		{
			SplashScreen splashScreen = null;
			var splashReadyEvent = new AutoResetEvent(false);
			var splashScreenThread = new Thread(() =>
			{
				splashScreen = new SplashScreen(settings, text);
				splashScreen.Closed += (o, args) => splashScreen.Dispatcher.InvokeShutdown();
				splashScreen.Show();

				splashReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			splashScreenThread.SetApartmentState(ApartmentState.STA);
			splashScreenThread.Name = "Splash Screen Thread";
			splashScreenThread.IsBackground = true;
			splashScreenThread.Start();

			splashReadyEvent.WaitOne();

			return splashScreen;
		}

		public ITaskbarNotification CreateNotification(INotificationInfo info)
		{
			return new NotificationIcon(info);
		}
	}
}
