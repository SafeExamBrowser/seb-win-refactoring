/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class IconResourceLoader
	{
		public static UIElement Load(IconResource resource)
		{
			try
			{
				switch (resource)
				{
					case BitmapIconResource bitmap:
						return LoadBitmapResource(bitmap);
					case EmbeddedIconResource embedded:
						return LoadEmbeddedResource(embedded);
					case NativeIconResource native:
						return LoadNativeResource(native);
					case XamlIconResource xaml:
						return LoadXamlResource(xaml);
					default:
						throw new NotSupportedException($"Application icon resource of type '{resource.GetType()}' is not supported!");
				}
			}
			catch (Exception)
			{
				return NotFoundSymbol();
			}
		}

		private static UIElement LoadBitmapResource(BitmapIconResource resource)
		{
			return new Image
			{
				Source = new BitmapImage(resource.Uri)
			};
		}

		private static UIElement LoadEmbeddedResource(EmbeddedIconResource resource)
		{
			using (var stream = new MemoryStream())
			{
				var bitmap = new BitmapImage();

				Icon.ExtractAssociatedIcon(resource.FilePath).ToBitmap().Save(stream, ImageFormat.Png);

				bitmap.BeginInit();
				bitmap.StreamSource = stream;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				bitmap.Freeze();

				return new Image
				{
					Source = bitmap
				};
			}
		}

		private static UIElement LoadNativeResource(NativeIconResource resource)
		{
			return new Image
			{
				Source = Imaging.CreateBitmapSourceFromHIcon(resource.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
			};
		}

		private static UIElement LoadXamlResource(XamlIconResource resource)
		{
			using (var stream = Application.GetResourceStream(resource.Uri)?.Stream)
			{
				return XamlReader.Load(stream) as UIElement;
			}
		}

		private static UIElement NotFoundSymbol()
		{
			return new TextBlock(new Run("X") { Foreground = Brushes.Red, FontWeight = FontWeights.Bold })
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
		}
	}
}
