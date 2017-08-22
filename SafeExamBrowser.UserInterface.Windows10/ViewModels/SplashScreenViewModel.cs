/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Timers;

namespace SafeExamBrowser.UserInterface.Windows10.ViewModels
{
	class SplashScreenViewModel : INotifyPropertyChanged
	{
		private int currentProgress;
		private bool isIndeterminate;
		private int maxProgress;
		private string status;
		private Timer busyTimer;

		public event PropertyChangedEventHandler PropertyChanged;

		public int CurrentProgress
		{
			get
			{
				return currentProgress;
			}
			set
			{
				currentProgress = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProgress)));
			}
		}

		public bool IsIndeterminate
		{
			get
			{
				return isIndeterminate;
			}
			set
			{
				isIndeterminate = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIndeterminate)));
			}
		}

		public int MaxProgress
		{
			get
			{
				return maxProgress;
			}
			set
			{
				maxProgress = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxProgress)));
			}
		}

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

			busyTimer = new Timer
			{
				AutoReset = true,
				Interval = 750
			};

			busyTimer.Elapsed += BusyTimer_Elapsed;
			busyTimer.Start();
		}

		public void StopBusyIndication()
		{
			busyTimer?.Stop();
			busyTimer?.Close();
		}

		private void BusyTimer_Elapsed(object sender, ElapsedEventArgs e)
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
