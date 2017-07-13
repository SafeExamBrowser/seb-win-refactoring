/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class TaskbarButton : UserControl, ITaskbarButton
	{
		public event TaskbarButtonClickHandler OnClick;

		public TaskbarButton(IApplicationInfo info)
		{
			InitializeComponent();
			InitializeButton(info);
		}

		public void RegisterInstance(Guid id, string title = null)
		{
			throw new NotImplementedException();
		}

		public void UnregisterInstance(Guid id)
		{
			throw new NotImplementedException();
		}

		private void InitializeButton(IApplicationInfo info)
		{
			Button.ToolTip = info.Tooltip;
			
			if (info.IconResource.IsBitmapResource)
			{
				var icon = new BitmapImage();
				var iconImage = new Image();

				icon.BeginInit();
				icon.UriSource = info.IconResource.Uri;
				icon.EndInit();

				iconImage.Source = icon;
				Button.Content = iconImage;
			}
			else if (info.IconResource.IsXamlResource)
			{
				using (var stream = Application.GetResourceStream(info.IconResource.Uri)?.Stream)
				{
					Button.Content = XamlReader.Load(stream);
				}
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// TODO
			OnClick?.Invoke();

			throw new NotImplementedException();
		}
	}
}
