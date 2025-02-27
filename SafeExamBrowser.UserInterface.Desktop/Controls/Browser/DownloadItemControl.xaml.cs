/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
		private readonly IText text;

		internal Guid Id { get; }

		internal DownloadItemControl(Guid id, IText text)
		{
			this.Id = id;
			this.text = text;

			InitializeComponent();
		}

		internal void Update(DownloadItemState state)
		{
			InitializeIcon(state);
			InitializeName(state);

			if (state.IsCancelled)
			{
				ShowCancelled();
			}
			else if (state.IsComplete)
			{
				ShowCompleted(state);
			}
			else
			{
				ShowProgress(state);
			}
		}

		private string BuildSizeInfo(DownloadItemState state)
		{
			return state.Size > 1000000 ? $"{state.Size / 1000000.0:N1} MB" : $"{state.Size / 1000.0:N1} kB";
		}

		private void InitializeIcon(DownloadItemState state)
		{
			if (Icon.Content == default && File.Exists(state.FullPath))
			{
				Icon.Content = new Image { Source = IconLoader.LoadIconFor(new FileInfo(state.FullPath)) };
			}
		}

		private void InitializeName(DownloadItemState state)
		{
			var fileName = Path.GetFileName(state.FullPath);

			if (ItemName.Text != fileName && File.Exists(state.FullPath))
			{
				ItemName.Text = fileName;
			}
			else if (string.IsNullOrEmpty(ItemName.Text))
			{
				ItemName.Text = Uri.TryCreate(state.Url, UriKind.Absolute, out var uri) ? Path.GetFileName(uri.AbsolutePath) : state.Url;
			}
		}

		private void ShowCancelled()
		{
			Progress.IsIndeterminate = false;
			Progress.Visibility = System.Windows.Visibility.Collapsed;
			Status.Text = text.Get(TextKey.BrowserWindow_DownloadCancelled);
		}

		private void ShowCompleted(DownloadItemState state)
		{
			Progress.IsIndeterminate = false;
			Progress.Visibility = System.Windows.Visibility.Collapsed;
			Status.Text = $"{BuildSizeInfo(state)} — {text.Get(TextKey.BrowserWindow_DownloadComplete)}";

			if (File.Exists(state.FullPath))
			{
				Icon.Content = new Image { Source = IconLoader.LoadIconFor(new FileInfo(state.FullPath)) };
				ItemName.Text = Path.GetFileName(state.FullPath);
			}
		}

		private void ShowProgress(DownloadItemState state)
		{
			Progress.IsIndeterminate = state.IsIndeterminate;
			Progress.Value = state.Completion * 100;
			Status.Text = $"{BuildSizeInfo(state)} — {text.Get(TextKey.BrowserWindow_Downloading)}{(state.IsIndeterminate ? "" : $" ({state.Completion * 100}%)")}";
		}
	}
}
