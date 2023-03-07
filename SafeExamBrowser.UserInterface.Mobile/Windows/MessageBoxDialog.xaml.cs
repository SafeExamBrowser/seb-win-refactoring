/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using MessageBoxResult = SafeExamBrowser.UserInterface.Contracts.MessageBox.MessageBoxResult;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class MessageBoxDialog : Window
	{
		private readonly IText text;

		public MessageBoxDialog(IText text)
		{
			this.text = text;

			InitializeComponent();
			InitializeMessageBox();
		}

		internal MessageBoxResult Show(string message, string title, MessageBoxAction action, MessageBoxIcon icon, Window parent = null)
		{
			Message.Text = message;
			Title = title;

			if (parent is Window)
			{
				Owner = parent as Window;
			}

			InitializeBounds();
			InitializeAction(action);
			InitializeIcon(icon);

			ShowDialog();

			return ResultFor(action);
		}

		private void InitializeAction(MessageBoxAction action)
		{
			switch (action)
			{
				case MessageBoxAction.Ok:
					CancelButton.Visibility = Visibility.Collapsed;
					OkButton.Visibility = Visibility.Visible;
					OkButton.Focus();
					YesButton.Visibility = Visibility.Collapsed;
					NoButton.Visibility = Visibility.Collapsed;
					break;
				case MessageBoxAction.OkCancel:
					CancelButton.Visibility = Visibility.Visible;
					CancelButton.Focus();
					OkButton.Visibility = Visibility.Visible;
					YesButton.Visibility = Visibility.Collapsed;
					NoButton.Visibility = Visibility.Collapsed;
					break;
				case MessageBoxAction.YesNo:
					CancelButton.Visibility = Visibility.Collapsed;
					OkButton.Visibility = Visibility.Collapsed;
					YesButton.Visibility = Visibility.Visible;
					NoButton.Visibility = Visibility.Visible;
					NoButton.Focus();
					break;
			}
		}

		private void InitializeBounds()
		{
			Left = 0;
			Top = 0;
			Height = SystemParameters.PrimaryScreenHeight;
			Width = SystemParameters.PrimaryScreenWidth;
		}

		private void InitializeIcon(MessageBoxIcon icon)
		{
			var handle = default(IntPtr);

			switch (icon)
			{
				case MessageBoxIcon.Error:
					handle = SystemIcons.Error.Handle;
					break;
				case MessageBoxIcon.Information:
					handle = SystemIcons.Information.Handle;
					break;
				case MessageBoxIcon.Question:
					handle = SystemIcons.Question.Handle;
					break;
				case MessageBoxIcon.Warning:
					handle = SystemIcons.Warning.Handle;
					break;
			}

			if (handle != default(IntPtr))
			{
				Image.Content = new System.Windows.Controls.Image
				{
					Source = Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
				};
			}
		}

		private void InitializeMessageBox()
		{
			InitializeBounds();

			CancelButton.Content = text.Get(TextKey.MessageBox_CancelButton);
			CancelButton.Click += CancelButton_Click;

			NoButton.Content = text.Get(TextKey.MessageBox_NoButton);
			NoButton.Click += NoButton_Click;

			OkButton.Content = text.Get(TextKey.MessageBox_OkButton);
			OkButton.Click += OkButton_Click;

			YesButton.Content = text.Get(TextKey.MessageBox_YesButton);
			YesButton.Click += YesButton_Click;

			SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
		}

		private MessageBoxResult ResultFor(MessageBoxAction action)
		{
			switch (action)
			{
				case MessageBoxAction.Ok:
					return DialogResult == true ? MessageBoxResult.Ok : MessageBoxResult.None;
				case MessageBoxAction.OkCancel:
					return DialogResult == true ? MessageBoxResult.Ok : MessageBoxResult.Cancel;
				case MessageBoxAction.YesNo:
					return DialogResult == true ? MessageBoxResult.Yes : MessageBoxResult.No;
				default:
					return MessageBoxResult.None;
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void NoButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
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
