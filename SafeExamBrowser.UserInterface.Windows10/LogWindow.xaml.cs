/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.UserInterface.Windows10.ViewModels;

namespace SafeExamBrowser.UserInterface.Windows10
{
	public partial class LogWindow : Window, IWindow
	{
		private ILogger logger;
		private LogViewModel model;
		private WindowClosingEventHandler closing;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public LogWindow(ILogger logger, IText text)
		{
			InitializeComponent();

			this.logger = logger;
			this.model = new LogViewModel(text, ScrollViewer, LogContent);

			InitializeLogWindow();
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
		}

		public new void Hide()
		{
			Dispatcher.Invoke(base.Hide);
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		private void InitializeLogWindow()
		{
			DataContext = model;
			Closing += LogWindow_Closing;
			Loaded += LogWindow_Loaded;
		}

		private void LogWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var log = logger.GetLog();

			foreach (var content in log)
			{
				model.Notify(content);
			}

			logger.Subscribe(model);
			logger.Info("Opened log window.");
		}

		private void LogWindow_Closing(object sender, CancelEventArgs e)
		{
			logger.Unsubscribe(model);
			logger.Info("Closed log window.");

			closing?.Invoke();
		}
	}
}
