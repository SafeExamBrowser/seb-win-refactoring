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

namespace SafeExamBrowser.UserInterface.Utilities
{
	internal static class ApplicationIconResourceLoader
	{
		internal static UIElement Load(IApplicationIconResource resource)
		{
			if (resource.IsBitmapResource)
			{
				var icon = new BitmapImage();
				var iconImage = new Image();

				icon.BeginInit();
				icon.UriSource = resource.Uri;
				icon.EndInit();

				iconImage.Source = icon;

				return iconImage;
			}
			else if (resource.IsXamlResource)
			{
				using (var stream = Application.GetResourceStream(resource.Uri)?.Stream)
				{
					return XamlReader.Load(stream) as UIElement;
				}
			}

			throw new NotSupportedException($"Application icon resource of type '{resource.GetType()}' is not supported!");
		}
	}
}
