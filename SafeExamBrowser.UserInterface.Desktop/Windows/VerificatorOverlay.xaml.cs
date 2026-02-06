/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class VerificatorOverlay : Window, IVerificatorOverlay
	{
		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

		private bool ignoredOnce;

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

		public VerificatorOverlay()
		{
			InitializeComponent();
			InitializeVerificatorOverlay();
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
		}

		public new void Hide()
		{
			Dispatcher.Invoke(base.Hide);
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		public void UpdateCode(byte[] data)
		{
			Dispatcher.Invoke(() =>
			{
				var image = new BitmapImage();

				using (var stream = new MemoryStream(data))
				{
					image.BeginInit();
					image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.StreamSource = stream;
					image.EndInit();
				}

				image.Freeze();

				Code.Source = image;
				Loading.Visibility = Visibility.Collapsed;
			});
		}

		private void InitializeVerificatorOverlay()
		{
			Closed += (o, args) => closed?.Invoke();
			Closing += (o, args) => closing?.Invoke();
			KeyDown += VerificatorOverlay_KeyDown;
			Loaded += VerificatorOverlay_Loaded;
			MouseDown += (o, args) => Close();
		}

		private void VerificatorOverlay_KeyDown(object sender, KeyEventArgs e)
		{
			var activationKey = e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.Up;

			if (activationKey && !ignoredOnce)
			{
				ignoredOnce = true;
			}
			else
			{
				Close();
			}
		}

		private void VerificatorOverlay_Loaded(object sender, RoutedEventArgs e)
		{
			Container.Width = Container.ActualHeight;
		}
	}
}
