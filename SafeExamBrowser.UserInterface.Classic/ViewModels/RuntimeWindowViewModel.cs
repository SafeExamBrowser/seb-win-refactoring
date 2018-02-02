/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;

namespace SafeExamBrowser.UserInterface.Classic.ViewModels
{
	internal class RuntimeWindowViewModel : ProgressIndicatorViewModel
	{
		private Visibility animatedBorderVisibility, progressBarVisibility;

		public Visibility AnimatedBorderVisibility
		{
			get
			{
				return animatedBorderVisibility;
			}
			set
			{
				animatedBorderVisibility = value;
				OnPropertyChanged(nameof(AnimatedBorderVisibility));
			}
		}

		public Visibility ProgressBarVisibility
		{
			get
			{
				return progressBarVisibility;
			}
			set
			{
				progressBarVisibility = value;
				OnPropertyChanged(nameof(ProgressBarVisibility));
			}
		}

		public override void StartBusyIndication()
		{
			base.StartBusyIndication();

			AnimatedBorderVisibility = Visibility.Hidden;
		}

		public override void StopBusyIndication()
		{
			base.StopBusyIndication();

			AnimatedBorderVisibility = Visibility.Visible;
		}
	}
}
