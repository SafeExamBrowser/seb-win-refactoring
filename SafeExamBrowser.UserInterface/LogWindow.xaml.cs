/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.ViewModels;

namespace SafeExamBrowser.UserInterface
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

		public LogWindow(ILogger logger, ILogContentFormatter formatter, IText text)
		{
			this.logger = logger;
			this.model = new LogViewModel(logger.GetLog(), formatter, text);

			InitializeComponent();
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

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		private void LogContent_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			LogContent.ScrollToEnd();
		}

		private void InitializeLogWindow()
		{
			DataContext = model;
			LogContent.DataContext = model;
			Closing += LogWindow_Closing;

			logger.Subscribe(model);
			logger.Info("Opened log window.");
		}

		private void LogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			logger.Unsubscribe(model);
			logger.Info("Closed log window.");

			closing?.Invoke();
		}
	}
}
