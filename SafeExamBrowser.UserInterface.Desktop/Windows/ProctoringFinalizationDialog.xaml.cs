/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Input;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class ProctoringFinalizationDialog : Window, IProctoringFinalizationDialog
	{
		private readonly IText text;

		public ProctoringFinalizationDialog(IText text)
		{
			this.text = text;

			InitializeComponent();
			InitializeDialog();
		}

		public new void Show()
		{
			Dispatcher.Invoke(() => ShowDialog());
		}

		public void Update(RemainingWorkUpdatedEventArgs status)
		{
			Dispatcher.Invoke(() =>
			{
				if (status.HasFailed)
				{
					ShowFailure(status);
				}
				else if (status.IsFinished)
				{
					Close();
				}
				else
				{
					ShowProgress(status);
				}
			});
		}

		private void InitializeDialog()
		{
			Button.Click += (o, args) => Close();
			Button.Content = text.Get(TextKey.ProctoringFinalizationDialog_Confirm);
			Loaded += (o, args) => this.DisableCloseButton();
			Title = text.Get(TextKey.ProctoringFinalizationDialog_Title);
		}

		private void ShowFailure(RemainingWorkUpdatedEventArgs status)
		{
			ButtonPanel.Visibility = Visibility.Visible;

			// TODO: Revert once cache handling has been specified and changed!
			CachePath.Text = status.CachePath ?? "-";
			CachePath.Visibility = Visibility.Collapsed;

			Cursor = Cursors.Arrow;
			FailurePanel.Visibility = Visibility.Visible;
			Message.Text = text.Get(TextKey.ProctoringFinalizationDialog_FailureMessage);
			ProgressPanel.Visibility = Visibility.Collapsed;

			this.EnableCloseButton();
		}

		private void ShowProgress(RemainingWorkUpdatedEventArgs status)
		{
			ButtonPanel.Visibility = Visibility.Collapsed;
			Cursor = Cursors.Wait;
			FailurePanel.Visibility = Visibility.Collapsed;
			Info.Text = text.Get(TextKey.ProctoringFinalizationDialog_InfoMessage);
			ProgressPanel.Visibility = Visibility.Visible;

			if (status.IsWaiting)
			{
				var count = $"{status.Total - status.Progress}";
				var time = $"{status.Resume.ToLongTimeString()}";

				Percentage.Text = "";
				Progress.IsIndeterminate = true;
				Status.Text = text.Get(TextKey.ProctoringFinalizationDialog_StatusWaiting).Replace("%%_COUNT_%%", count).Replace("%%_TIME_%%", time);
			}
			else
			{
				var count = $"{status.Progress}";
				var total = $"{status.Total}";

				Percentage.Text = $"{status.Progress / (double) (status.Total > 0 ? status.Total : 1) * 100:N0}%";
				Progress.IsIndeterminate = false;
				Progress.Maximum = status.Total;
				Progress.Value = status.Progress;

				if (status.Next.HasValue)
				{
					Status.Text = text.Get(TextKey.ProctoringFinalizationDialog_StatusAndTime).Replace("%%_TIME_%%", $"{status.Next.Value.ToLongTimeString()}");
				}
				else
				{
					Status.Text = text.Get(TextKey.ProctoringFinalizationDialog_Status);
				}

				Status.Text = Status.Text.Replace("%%_COUNT_%%", count).Replace("%%_TOTAL_%%", total);
			}
		}
	}
}
