/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Windows.Controls;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.Browser
{
	internal partial class DownloadItemControl : UserControl
	{
		private IText text;

		internal Guid Id { get; }

		internal DownloadItemControl(Guid id, IText text)
		{
			this.Id = id;
			this.text = text;

			InitializeComponent();
		}

		internal void Update(DownloadItemState state)
		{
			ItemName.Text = Uri.TryCreate(state.Url, UriKind.Absolute, out var uri) ? Path.GetFileName(uri.AbsolutePath) : state.Url;
			Progress.Value = state.Completion * 100;
			Status.Text = $"{text.Get(TextKey.BrowserWindow_Downloading)} ({state.Completion * 100}%)";

			if (File.Exists(state.FullPath))
			{
				ItemName.Text = Path.GetFileName(state.FullPath);
				Icon.Content = new Image { Source = IconLoader.LoadIconFor(new FileInfo(state.FullPath)) };
			}

			if (state.IsCancelled)
			{
				Progress.Visibility = System.Windows.Visibility.Collapsed;
				Status.Text = text.Get(TextKey.BrowserWindow_DownloadCancelled);
			}
			else if (state.IsComplete)
			{
				Progress.Visibility = System.Windows.Visibility.Collapsed;
				Status.Text = text.Get(TextKey.BrowserWindow_DownloadComplete);
			}
		}
	}
}
