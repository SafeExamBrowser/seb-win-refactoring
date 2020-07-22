/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class ExamSelectionDialog : Window, IExamSelectionDialog
	{
		private readonly IText text;

		public ExamSelectionDialog(string message, string title, IText text, IEnumerable<Exam> exams)
		{
			this.text = text;

			InitializeComponent();
			InitializeExamSelectionDialog(message, title, exams);
		}

		public ExamSelectionDialogResult Show(IWindow parent = null)
		{
			return Dispatcher.Invoke(() =>
			{
				var result = new ExamSelectionDialogResult { Success = false };

				if (parent is Window)
				{
					Owner = parent as Window;
					WindowStartupLocation = WindowStartupLocation.CenterOwner;
				}

				if (ShowDialog() is true)
				{
					result.SelectedExam = ExamList.SelectedItem as Exam;
					result.Success = true;
				}

				return result;
			});
		}

		private void InitializeExamSelectionDialog(string message, string title, IEnumerable<Exam> exams)
		{
			Message.Text = message;
			Title = title;
			WindowStartupLocation = WindowStartupLocation.CenterScreen;

			CancelButton.Content = text.Get(TextKey.ExamSelectionDialog_Cancel);
			CancelButton.Click += CancelButton_Click;

			SelectButton.Content = text.Get(TextKey.ExamSelectionDialog_Select);
			SelectButton.Click += ConfirmButton_Click;

			ExamList.ItemsSource = exams;
			ExamList.SelectionChanged += ExamList_SelectionChanged;

			Loaded += (o, args) => Activate();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void ExamList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectButton.IsEnabled = ExamList.SelectedItem != null;
		}
	}
}
