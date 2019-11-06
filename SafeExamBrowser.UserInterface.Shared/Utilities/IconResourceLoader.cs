/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Core.Contracts;
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
				switch (resource.Type)
				{
					case IconResourceType.Bitmap:
						return LoadBitmapResource(resource);
					case IconResourceType.Embedded:
						return LoadEmbeddedResource(resource);
					case IconResourceType.Xaml:
						return LoadXamlResource(resource);
					default:
						throw new NotSupportedException($"Application icon resource of type '{resource.Type}' is not supported!");
				}
			}
			catch (Exception)
			{
				return NotFoundSymbol();
			}
		}

		private static UIElement LoadBitmapResource(IconResource resource)
		{
			return new Image
			{
				Source = new BitmapImage(resource.Uri)
			};
		}

		private static UIElement LoadEmbeddedResource(IconResource resource)
		{
			using (var stream = new MemoryStream())
			{
				var bitmap = new BitmapImage();

				Icon.ExtractAssociatedIcon(resource.Uri.LocalPath).ToBitmap().Save(stream, ImageFormat.Png);

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

		private static UIElement LoadXamlResource(IconResource resource)
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
