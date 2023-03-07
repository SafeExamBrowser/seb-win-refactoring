/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace SafeExamBrowser.UserInterface.Mobile.ViewModels
{
	internal class DateTimeViewModel : INotifyPropertyChanged
	{
		private DispatcherTimer timer;
		private readonly bool showSeconds;

		public string Date { get; private set; }
		public string Time { get; private set; }
		public string ToolTip { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public DateTimeViewModel(bool showSeconds)
		{
			this.showSeconds = showSeconds;
			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(250);
			this.timer.Tick += Timer_Tick;
			this.timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			var date = DateTime.Now;

			Date = date.ToShortDateString();
			Time = showSeconds ? date.ToLongTimeString() : date.ToShortTimeString();
			ToolTip = $"{date.ToLongDateString()} {date.ToLongTimeString()}";

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTip)));
		}
	}
}
