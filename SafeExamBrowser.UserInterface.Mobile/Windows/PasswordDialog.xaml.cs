/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Input;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class PasswordDialog : Window, IPasswordDialog
	{
		private readonly IText text;

		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

		event WindowClosedEventHandler IWindow.Closed
		{
			add { closed += value; }
			remove { closed -= value; }
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		internal PasswordDialog(string message, string title, IText text)
		{
			this.text = text;

			InitializeComponent();
			InitializePasswordDialog(message, title);
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public PasswordDialogResult Show(IWindow parent = null)
		{
			return Dispatcher.Invoke(() =>
			{
				var result = new PasswordDialogResult { Success = false };

				if (parent is Window)
				{
					Owner = parent as Window;
				}

				InitializeBounds();

				if (ShowDialog() is true)
				{
					result.Password = Password.Password;
					result.Success = true;
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

		private void InitializePasswordDialog(string message, string title)
		{
			InitializeBounds();

			Message.Text = message;
			Title = title;

			Closed += (o, args) => closed?.Invoke();
			Closing += (o, args) => closing?.Invoke();
			Loaded += (o, args) => Activate();

			CancelButton.Content = text.Get(TextKey.PasswordDialog_Cancel);
			CancelButton.Click += CancelButton_Click;

			ConfirmButton.Content = text.Get(TextKey.PasswordDialog_Confirm);
			ConfirmButton.Click += ConfirmButton_Click;

			Password.KeyDown += Password_KeyDown;

			SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
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

		private void Password_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				DialogResult = true;
				Close();
			}
		}

		private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SystemParameters.WorkArea))
			{
				Dispatcher.InvokeAsync(InitializeBounds);
			}
		}
	}
}
