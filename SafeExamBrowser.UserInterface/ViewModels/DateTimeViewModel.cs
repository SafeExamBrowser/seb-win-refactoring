/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Timers;

namespace SafeExamBrowser.UserInterface.ViewModels
{
	class DateTimeViewModel : INotifyPropertyChanged
	{
		private Timer timer;

		public event PropertyChangedEventHandler PropertyChanged;

		public DateTimeViewModel()
		{
			timer = new Timer(1000);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
		}

		public string Time { get; private set; }
		public string Date { get; private set; }

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Time = DateTime.Now.ToShortTimeString();
			Date = DateTime.Now.ToShortDateString();

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
		}
	}
}
