/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Timers;

namespace SafeExamBrowser.UserInterface.Classic.ViewModels
{
	internal class RuntimeWindowViewModel : INotifyPropertyChanged
	{
		private string status;
		private Timer timer;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				status = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
			}
		}

		public void StartBusyIndication()
		{
			StopBusyIndication();

			timer = new Timer
			{
				AutoReset = true,
				Interval = 750
			};

			timer.Elapsed += Timer_Elapsed;
			timer.Start();
		}

		public void StopBusyIndication()
		{
			timer?.Stop();
			timer?.Close();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var next = Status ?? string.Empty;

			if (next.EndsWith("..."))
			{
				next = Status.Substring(0, Status.Length - 3);
			}
			else
			{
				next += ".";
			}

			Status = next;
		}
	}
}
