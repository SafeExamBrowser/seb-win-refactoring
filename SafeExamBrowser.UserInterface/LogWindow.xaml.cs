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
			InitializeComponent();

			this.logger = logger;
			this.model = new LogViewModel(logger.GetLog(), formatter, text);

			DataContext = model;
			LogContent.DataContext = model;

			logger.Subscribe(model);
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public new void Close()
		{
			Dispatcher.Invoke(() =>
			{
				logger.Unsubscribe(model);
				base.Close();
			});
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		private void LogContent_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			LogContent.ScrollToEnd();
		}
	}
}
