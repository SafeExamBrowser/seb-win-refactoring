/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Timers;

namespace SafeExamBrowser.UserInterface.Desktop.ViewModels
{
	internal class ProgressIndicatorViewModel : INotifyPropertyChanged
	{
		private readonly object @lock = new object();

		private Timer busyTimer;
		private int currentProgress;
		private bool isIndeterminate;
		private int maxProgress;
		private string status;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool BusyIndication
		{
			set
			{
				HandleBusyIndication(value);
			}
		}

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

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void HandleBusyIndication(bool start)
		{
			lock (@lock)
			{
				if (busyTimer != null)
				{
					busyTimer.Elapsed -= BusyTimer_Elapsed;
					busyTimer.Stop();
					busyTimer.Close();
				}

				if (start)
				{
					busyTimer = new Timer
					{
						AutoReset = true,
						Interval = 1500,
					};
					busyTimer.Elapsed += BusyTimer_Elapsed;
					busyTimer.Start();
				}
			}
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
			busyTimer.Interval = 750;
		}
	}
}
