/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FontAwesome.WPF;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class FileSystemDialog : Window, IFileSystemDialog
	{
		private FileSystemElement element;
		private string initialPath;
		private string message;
		private FileSystemOperation operation;
		private IText text;
		private string title;

		public FileSystemDialog(
			FileSystemElement element,
			string initialPath,
			FileSystemOperation operation,
			IText text,
			string message = default(string),
			string title = default(string))
		{
			this.element = element;
			this.initialPath = initialPath;
			this.message = message;
			this.operation = operation;
			this.text = text;
			this.title = title;

			InitializeComponent();
			InitializeDialog();
		}

		public FileSystemDialogResult Show(IWindow parent = null)
		{
			return Dispatcher.Invoke(() =>
			{
				var result = new FileSystemDialogResult();

				if (parent is Window)
				{
					Owner = parent as Window;
					WindowStartupLocation = WindowStartupLocation.CenterOwner;
				}

				if (ShowDialog() == true)
				{
					result.FullPath = BuildFullPath();
					result.Success = true;
				}

				return result;
			});
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void FileSystem_Expanded(object sender, RoutedEventArgs e)
		{
			if (e.Source is TreeViewItem item && item.Items.Count == 1 && !(item.Items[0] is TreeViewItem))
			{
				Load(item);
			}
		}

		private void FileSystem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is TreeViewItem item)
			{
				if (item.Tag is DirectoryInfo directory)
				{
					SelectButton.IsEnabled = element == FileSystemElement.Folder || operation == FileSystemOperation.Save;
					SelectedElement.Text = directory.FullName;
					SelectedElement.ToolTip = directory.FullName;
				}
				else if (item.Tag is FileInfo file)
				{
					SelectButton.IsEnabled = element == FileSystemElement.File;
					SelectedElement.Text = file.FullName;
					SelectedElement.ToolTip = file.FullName;
				}
				else
				{
					SelectButton.IsEnabled = false;
				}
			}
		}

		private void NewElementName_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && IsValid())
			{
				DialogResult = true;
				Close();
			}
		}

		private void SelectButton_Click(object sender, RoutedEventArgs e)
		{
			if (IsValid())
			{
				DialogResult = true;
				Close();
			}
		}

		private string BuildFullPath()
		{
			var fullPath = SelectedElement.Text;

			if (operation == FileSystemOperation.Save)
			{
				fullPath = Path.Combine(SelectedElement.Text, NewElementName.Text);

				if (element == FileSystemElement.File)
				{
					var extension = Path.GetExtension(initialPath);

					if (!fullPath.EndsWith(extension))
					{
						fullPath = $"{fullPath}{extension}";
					}
				}
			}

			return fullPath;
		}

		private bool IsValid()
		{
			var fullPath = BuildFullPath();
			var isValid = true;

			if (element == FileSystemElement.File && operation == FileSystemOperation.Save && File.Exists(fullPath))
			{
				var message = text.Get(TextKey.FileSystemDialog_OverwriteWarning);
				var title = text.Get(TextKey.FileSystemDialog_OverwriteWarningTitle);
				var result = System.Windows.MessageBox.Show(this, message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

				isValid = result == MessageBoxResult.Yes;
			}

			return isValid;
		}

		private void Load(TreeViewItem item)
		{
			item.Items.Clear();

			if (item.Tag is DirectoryInfo directory)
			{
				FileSystem.Cursor = Cursors.Wait;
				item.BeginInit();

				try
				{
					foreach (var subDirectory in directory.GetDirectories())
					{
						if (!subDirectory.Attributes.HasFlag(FileAttributes.Hidden))
						{
							item.Items.Add(CreateItem(subDirectory));
						}
					}

					foreach (var file in directory.GetFiles())
					{
						if (!file.Attributes.HasFlag(FileAttributes.Hidden))
						{
							item.Items.Add(CreateItem(file));
						}
					}
				}
				catch (Exception e)
				{
					item.Items.Add(CreateErrorItem(e));
				}

				item.EndInit();
				FileSystem.Cursor = Cursors.Arrow;
			}
		}

		private TreeViewItem CreateErrorItem(Exception e)
		{
			var item = new TreeViewItem();

			item.Foreground = Brushes.Red;
			item.Header = $"{text.Get(TextKey.FileSystemDialog_LoadError)} {e.Message}";
			item.ToolTip = e.GetType() + Environment.NewLine + e.StackTrace;

			return item;
		}

		private TreeViewItem CreateItem(DirectoryInfo directory)
		{
			var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
			var image = new Image
			{
				Height = 16,
				Source = IconLoader.LoadIconFor(directory)
			};
			var item = new TreeViewItem();
			var textBlock = new TextBlock { Margin = new Thickness(5, 0, 0, 0), Text = directory.Name, VerticalAlignment = VerticalAlignment.Center };

			header.Children.Add(image);
			header.Children.Add(textBlock);

			item.Cursor = Cursors.Hand;
			item.Header = header;
			item.Tag = directory;
			item.ToolTip = directory.FullName;
			item.Items.Add(text.Get(TextKey.FileSystemDialog_Loading));

			return item;
		}

		private TreeViewItem CreateItem(FileInfo file)
		{
			var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
			var image = new Image
			{
				Height = 20,
				Source = IconLoader.LoadIconFor(file)
			};
			var item = new TreeViewItem();
			var textBlock = new TextBlock { Margin = new Thickness(5, 0, 0, 0), Text = file.Name, VerticalAlignment = VerticalAlignment.Center };

			header.Children.Add(image);
			header.Children.Add(textBlock);

			item.Header = header;
			item.Tag = file;
			item.ToolTip = file.FullName;

			if (element == FileSystemElement.File && operation == FileSystemOperation.Open)
			{
				item.Cursor = Cursors.Hand;
			}
			else
			{
				item.Cursor = Cursors.No;
				item.Focusable = false;
				textBlock.Foreground = Brushes.Gray;
			}

			return item;
		}

		private void InitializeDialog()
		{
			CancelButton.Click += CancelButton_Click;
			CancelButton.Content = text.Get(TextKey.FileSystemDialog_Cancel);
			FileSystem.SelectedItemChanged += FileSystem_SelectedItemChanged;
			NewElement.Visibility = operation == FileSystemOperation.Save ? Visibility.Visible : Visibility.Collapsed;
			NewElementLabel.Text = text.Get(TextKey.FileSystemDialog_SaveAs);
			NewElementName.KeyUp += NewElementName_KeyUp;
			OperationIcon.Icon = operation == FileSystemOperation.Save ? FontAwesomeIcon.Download : FontAwesomeIcon.Search;
			SelectButton.Click += SelectButton_Click;
			SelectButton.Content = text.Get(TextKey.FileSystemDialog_Select);

			InitializeText();
			InitializeFileSystem();
		}

		private void InitializeFileSystem()
		{
			foreach (var drive in DriveInfo.GetDrives())
			{
				FileSystem.Items.Add(CreateItem(drive.RootDirectory));
			}

			if (FileSystem.HasItems && FileSystem.Items[0] is TreeViewItem item)
			{
				item.IsSelected = true;
			}

			if (!string.IsNullOrEmpty(initialPath))
			{
				var pathRoot = Path.GetPathRoot(initialPath);
				var directories = initialPath.Replace(pathRoot, "").Split(Path.DirectorySeparatorChar);
				var segments = new List<string>();

				segments.Add(pathRoot);
				segments.AddRange(directories);

				AutoSelect(FileSystem.Items, segments);

				if (element == FileSystemElement.File && operation == FileSystemOperation.Save)
				{
					NewElementName.Text = Path.GetFileName(initialPath);
				}
			}
		}

		private void AutoSelect(ItemCollection items, List<string> segments)
		{
			var segment = segments.FirstOrDefault();

			if (segment != default(string))
			{
				foreach (var item in items)
				{
					if (item is TreeViewItem i && i.Tag is DirectoryInfo d && d.Name.Equals(segment))
					{
						i.IsExpanded = true;
						i.IsSelected = true;
						i.BringIntoView();

						AutoSelect(i.Items, segments.Skip(1).ToList());

						break;
					}
				}
			}
		}

		private void InitializeText()
		{
			if (string.IsNullOrEmpty(message))
			{
				if (element == FileSystemElement.File)
				{
					if (operation == FileSystemOperation.Open)
					{
						Message.Text = text.Get(TextKey.FileSystemDialog_OpenFileMessage);
					}
					else
					{
						Message.Text = text.Get(TextKey.FileSystemDialog_SaveFileMessage);
					}
				}
				else
				{
					if (operation == FileSystemOperation.Open)
					{
						Message.Text = text.Get(TextKey.FileSystemDialog_OpenFolderMessage);
					}
					else
					{
						Message.Text = text.Get(TextKey.FileSystemDialog_SaveFolderMessage);
					}
				}
			}
			else
			{
				Message.Text = message;
			}

			if (string.IsNullOrEmpty(title))
			{
				Title = text.Get(TextKey.FileSystemDialog_Title);
			}
			else
			{
				Title = title;
			}
		}
	}
}
