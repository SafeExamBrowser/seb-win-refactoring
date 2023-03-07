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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Desktop.ViewModels;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	internal partial class RuntimeWindow : Window, IRuntimeWindow
	{
		private readonly AppConfig appConfig;
		private readonly IText text;

		private bool allowClose;
		private RuntimeWindowViewModel model;

		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

		public bool ShowLog
		{
			set => Dispatcher.Invoke(() => LogScrollViewer.Visibility = value ? Visibility.Visible : Visibility.Collapsed);
		}

		public bool ShowProgressBar
		{
			set => Dispatcher.Invoke(() => model.ProgressBarVisibility = value ? Visibility.Visible : Visibility.Hidden);
		}

		public bool TopMost
		{
			set => Dispatcher.Invoke(() => Topmost = value);
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

		internal RuntimeWindow(AppConfig appConfig, IText text)
		{
			this.appConfig = appConfig;
			this.text = text;

			InitializeComponent();
			InitializeRuntimeWindow();
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

		public void Notify(ILogContent content)
		{
			Dispatcher.Invoke(() =>
			{
				model.Notify(content);
				LogScrollViewer.ScrollToEnd();
			});
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

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		private void InitializeRuntimeWindow()
		{
			Title = $"{appConfig.ProgramTitle} - Version {appConfig.ProgramInformationalVersion}";

			InfoTextBlock.Inlines.Add(new Run($"Version {appConfig.ProgramInformationalVersion}") { FontSize = 12 });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run($"Build {appConfig.ProgramBuildVersion}") { FontSize = 8, Foreground = Brushes.Gray });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run(appConfig.ProgramCopyright) { FontSize = 10, Foreground = Brushes.Gray });

			model = new RuntimeWindowViewModel(LogTextBlock);
			ProgressBar.DataContext = model;
			StatusTextBlock.DataContext = model;

			Closed += (o, args) => closed?.Invoke();
			Closing += (o, args) => args.Cancel = !allowClose;

#if DEBUG
			Topmost = false;
#endif
		}
	}
}
