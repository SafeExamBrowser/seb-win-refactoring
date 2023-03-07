/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskview
{
	internal partial class WindowControl : UserControl
	{
		private Windows.Taskview taskview;
		private IntPtr thumbnail;
		private IApplicationWindow window;

		internal WindowControl(IApplicationWindow window, Windows.Taskview taskview)
		{
			this.window = window;
			this.taskview = taskview;

			InitializeComponent();
			InitializeControl();
		}

		internal void Activate()
		{
			window.Activate();
		}

		internal void Deselect()
		{
			Indicator.Visibility = Visibility.Hidden;
			Title.FontWeight = FontWeights.Normal;
		}

		internal void Destroy()
		{
			if (thumbnail != IntPtr.Zero)
			{
				Thumbnail.DwmUnregisterThumbnail(thumbnail);
			}
		}

		internal void Select()
		{
			Indicator.Visibility = Visibility.Visible;
			Title.FontWeight = FontWeights.SemiBold;
		}

		internal void Update()
		{
			if (!IsLoaded || !IsVisible)
			{
				return;
			}

			if (thumbnail == IntPtr.Zero && taskview.Handle != IntPtr.Zero && window.Handle != IntPtr.Zero)
			{
				Thumbnail.DwmRegisterThumbnail(taskview.Handle, window.Handle, out thumbnail);
			}

			if (thumbnail != IntPtr.Zero)
			{
				Thumbnail.DwmQueryThumbnailSourceSize(thumbnail, out var size);

				var destination = CalculatePhysicalDestination(size);
				var properties = new Thumbnail.Properties
				{
					Destination = destination,
					Flags = Thumbnail.DWM_TNP_RECTDESTINATION | Thumbnail.DWM_TNP_VISIBLE,
					Visible = true
				};

				Thumbnail.DwmUpdateThumbnailProperties(thumbnail, ref properties);
			}
		}

		private void TaskViewWindowControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue as bool? == true)
			{
				Update();
			}
		}

		private void Window_TitleChanged(string title)
		{
			Dispatcher.InvokeAsync(() => Title.Text = title);
		}

		private void Window_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(window.Icon));
		}

		private Thumbnail.Rectangle CalculatePhysicalDestination(Thumbnail.Size size)
		{
			var controlToTaskview = TransformToVisual(taskview);
			var placeholderToControl = Placeholder.TransformToVisual(this);
			var placeholderLeft = placeholderToControl.Transform(new Point(0, 0)).X;
			var placeholderTop = placeholderToControl.Transform(new Point(0, 0)).Y;
			var placeholderRight = placeholderToControl.Transform(new Point(Placeholder.ActualWidth, 0)).X;
			var placeholderBottom = placeholderToControl.Transform(new Point(0, Placeholder.ActualHeight)).Y;

			var physicalBounds = new Thumbnail.Rectangle
			{
				Left = (int) Math.Round(this.TransformToPhysical(controlToTaskview.Transform(new Point(placeholderLeft, 0)).X, 0).X),
				Top = (int) Math.Round(this.TransformToPhysical(0, controlToTaskview.Transform(new Point(0, placeholderTop)).Y).Y),
				Right = (int) Math.Round(this.TransformToPhysical(controlToTaskview.Transform(new Point(placeholderRight, 0)).X, 0).X),
				Bottom = (int) Math.Round(this.TransformToPhysical(0, controlToTaskview.Transform(new Point(0, placeholderBottom)).Y).Y)
			};

			var scaleFactor = default(double);
			var thumbnailHeight = default(double);
			var thumbnailWidth = default(double);
			var maxWidth = (double) physicalBounds.Right - physicalBounds.Left;
			var maxHeight = (double) physicalBounds.Bottom - physicalBounds.Top;
			var placeholderRatio = maxWidth / maxHeight;
			var windowRatio = (double) size.X / size.Y;

			if (windowRatio < placeholderRatio)
			{
				thumbnailHeight = maxHeight;
				scaleFactor = thumbnailHeight / size.Y;
				thumbnailWidth = size.X * scaleFactor;
			}
			else
			{
				thumbnailWidth = maxWidth;
				scaleFactor = thumbnailWidth / size.X;
				thumbnailHeight = size.Y * scaleFactor;
			}

			var widthDifference = maxWidth - thumbnailWidth;
			var heightDifference = maxHeight - thumbnailHeight;

			return new Thumbnail.Rectangle
			{
				Left = (int) Math.Round(physicalBounds.Left + (widthDifference / 2)),
				Top = (int) Math.Round(physicalBounds.Top + (heightDifference / 2)),
				Right = (int) Math.Round(physicalBounds.Right - (widthDifference / 2)),
				Bottom = (int) Math.Round(physicalBounds.Bottom - (heightDifference / 2))
			};
		}

		private void InitializeControl()
		{
			Icon.Content = IconResourceLoader.Load(window.Icon);
			IsVisibleChanged += TaskViewWindowControl_IsVisibleChanged;
			Loaded += (o, args) => Update();
			Title.Text = window.Title;
			window.IconChanged += Window_IconChanged;
			window.TitleChanged += Window_TitleChanged;
		}
	}
}
