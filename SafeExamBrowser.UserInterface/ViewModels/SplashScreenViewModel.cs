/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;

namespace SafeExamBrowser.UserInterface.ViewModels
{
	class SplashScreenViewModel : INotifyPropertyChanged
	{
		private int currentProgress;
		private int maxProgress;
		private string status;

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
	}
}
