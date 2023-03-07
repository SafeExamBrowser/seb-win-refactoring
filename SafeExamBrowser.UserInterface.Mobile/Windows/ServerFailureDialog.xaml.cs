/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	public partial class ServerFailureDialog : Window, IServerFailureDialog
	{
		private readonly IText text;

		public ServerFailureDialog(string info, bool showFallback, IText text)
		{
			this.text = text;

			InitializeComponent();
			InitializeDialog(info, showFallback);
		}

		public ServerFailureDialogResult Show(IWindow parent = null)
		{
			return Dispatcher.Invoke(() =>
			{
				var result = new ServerFailureDialogResult { Success = false };

				if (parent is Window)
				{
					Owner = parent as Window;
					WindowStartupLocation = WindowStartupLocation.CenterOwner;
				}

				InitializeBounds();

				if (ShowDialog() is true)
				{
					result.Abort = Tag as string == nameof(AbortButton);
					result.Fallback = Tag as string == nameof(FallbackButton);
					result.Retry = Tag as string == nameof(RetryButton);
					result.Success = true;
				}
				else
				{
					result.Abort = true;
				}

				return result;
			});
		}

		private void InitializeBounds()
		{
			Left = 0;
			Top = 0;
			Height = SystemParameters.PrimaryScreenHeight;
			Width = SystemParameters.PrimaryScreenWidth;
		}

		private void InitializeDialog(string info, bool showFallback)
		{
			InitializeBounds();

			Info.Text = info;
			Message.Text = text.Get(TextKey.ServerFailureDialog_Message);
			Title = text.Get(TextKey.ServerFailureDialog_Title);

			AbortButton.Click += AbortButton_Click;
			AbortButton.Content = text.Get(TextKey.ServerFailureDialog_Abort);

			FallbackButton.Click += FallbackButton_Click;
			FallbackButton.Content = text.Get(TextKey.ServerFailureDialog_Fallback);
			FallbackButton.Visibility = showFallback ? Visibility.Visible : Visibility.Collapsed;

			Loaded += (o, args) => Activate();

			RetryButton.Click += RetryButton_Click;
			RetryButton.Content = text.Get(TextKey.ServerFailureDialog_Retry);

			SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
		}

		private void AbortButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Tag = nameof(AbortButton);
			Close();
		}

		private void FallbackButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Tag = nameof(FallbackButton);
			Close();
		}

		private void RetryButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Tag = nameof(RetryButton);
			Close();
		}

		private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SystemParameters.WorkArea))
			{
				Dispatcher.InvokeAsync(InitializeBounds);
			}
		}
	}
}
