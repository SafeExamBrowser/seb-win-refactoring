/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.UserInterface.Windows10.ViewModels;

namespace SafeExamBrowser.UserInterface.Windows10
{
	public partial class SplashScreen : Window, ISplashScreen
	{
		private bool allowClose;
		private SplashScreenViewModel model = new SplashScreenViewModel();
		private AppConfig appConfig;
		private IText text;
		private WindowClosingEventHandler closing;

		public AppConfig AppConfig
		{
			set
			{
				Dispatcher.Invoke(() =>
				{
					appConfig = value;
					UpdateAppInfo();
				});
			}
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public SplashScreen(IText text, AppConfig appConfig = null)
		{
			this.appConfig = appConfig;
			this.text = text;

			InitializeComponent();
			InitializeSplashScreen();
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public new void Close()
		{
			Dispatcher.Invoke(() =>
			{
				allowClose = true;
				base.Close();
			});
		}

		public new void Hide()
		{
			Dispatcher.Invoke(base.Hide);
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		public void Progress()
		{
			model.CurrentProgress += 1;
		}

		public void Regress()
		{
			model.CurrentProgress -= 1;
		}

		public void SetIndeterminate()
		{
			model.IsIndeterminate = true;
		}

		public void SetMaxValue(int max)
		{
			model.MaxProgress = max;
		}

		public void SetValue(int value)
		{
			model.CurrentProgress = value;
		}

		public void UpdateText(TextKey key, bool showBusyIndication = false)
		{
			model.StopBusyIndication();
			model.Status = text.Get(key);

			if (showBusyIndication)
			{
				model.StartBusyIndication();
			}
		}

		private void InitializeSplashScreen()
		{
			UpdateAppInfo();

			StatusTextBlock.DataContext = model;
			ProgressBar.DataContext = model;

			// To prevent the progress bar going from max to min value at startup...
			model.MaxProgress = 1;

			Closing += (o, args) => args.Cancel = !allowClose;
		}

		private void UpdateAppInfo()
		{
			if (appConfig != null)
			{
				InfoTextBlock.Inlines.Add(new Run($"Version {appConfig.ProgramVersion}") { FontStyle = FontStyles.Italic });
				InfoTextBlock.Inlines.Add(new LineBreak());
				InfoTextBlock.Inlines.Add(new LineBreak());
				InfoTextBlock.Inlines.Add(new Run(appConfig.ProgramCopyright) { FontSize = 10 });
			}
		}
	}
}
