/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Mobile.ViewModels;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class SplashScreen : Window, ISplashScreen
	{
		private readonly ProgressIndicatorViewModel model;
		private readonly IText text;

		private bool allowClose;
		private AppConfig appConfig;

		private WindowClosedEventHandler closed;
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

		event WindowClosedEventHandler IWindow.Closed
		{
			add { closed += value; }
			remove { closed -= value; }
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		internal SplashScreen(IText text, AppConfig appConfig = null)
		{
			this.appConfig = appConfig;
			this.model = new ProgressIndicatorViewModel();
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
				model.BusyIndication = false;
				closing?.Invoke();

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

		public void UpdateStatus(TextKey key, bool busyIndication = false)
		{
			model.Status = text.Get(key);
			model.BusyIndication = busyIndication;
		}

		private void InitializeSplashScreen()
		{
			UpdateAppInfo();

			StatusTextBlock.DataContext = model;
			ProgressBar.DataContext = model;

			Closed += (o, args) => closed?.Invoke();
			Closing += (o, args) => args.Cancel = !allowClose;
		}

		private void UpdateAppInfo()
		{
			if (appConfig != null)
			{
				InfoTextBlock.Inlines.Add(new Run($"Version {appConfig.ProgramInformationalVersion}") { Foreground = Brushes.DimGray });
				InfoTextBlock.Inlines.Add(new LineBreak());
				InfoTextBlock.Inlines.Add(new LineBreak());
				InfoTextBlock.Inlines.Add(new Run(appConfig.ProgramCopyright) { FontSize = 12 });
			}
		}
	}
}
