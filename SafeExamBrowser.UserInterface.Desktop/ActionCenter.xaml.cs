/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Media.Animation;
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class ActionCenter : Window, IActionCenter
	{
		public ActionCenter()
		{
			InitializeComponent();
			RegisterEvents();
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
		}

		public new void Hide()
		{
			Dispatcher.Invoke(HideAnimated);
		}

		public void Register(IActionCenterActivator activator)
		{
			activator.Activate += Activator_Activate;
			activator.Deactivate += Activator_Deactivate;
			activator.Toggle += Activator_Toggle;
		}

		public new void Show()
		{
			Dispatcher.Invoke(ShowAnimated);
		}

		private void HideAnimated()
		{
			var storyboard = new Storyboard();
			var animation = new DoubleAnimation
			{
				EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut },
				From = 0,
				To = -Width,
				Duration = new Duration(TimeSpan.FromMilliseconds(500))
			};

			Storyboard.SetTarget(animation, this);
			Storyboard.SetTargetProperty(animation, new PropertyPath(LeftProperty));

			storyboard.Children.Add(animation);
			storyboard.Completed += (o, args) => Dispatcher.Invoke(base.Hide);
			storyboard.Begin();
		}

		private void ShowAnimated()
		{
			var storyboard = new Storyboard();
			var animation = new DoubleAnimation
			{
				EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut },
				From = -Width,
				To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(500))
			};

			Storyboard.SetTarget(animation, this);
			Storyboard.SetTargetProperty(animation, new PropertyPath(LeftProperty));

			InitializeBounds();
			base.Show();
			Activate();

			storyboard.Children.Add(animation);
			storyboard.Completed += (o, args) => Dispatcher.Invoke(Activate);
			storyboard.Begin();
		}

		private void InitializeBounds()
		{
			Height = SystemParameters.WorkArea.Height;
			Top = 0;
			Left = -Width;
		}

		private void RegisterEvents()
		{
			Deactivated += (o, args) => HideAnimated();
		}

		private void Activator_Activate()
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (Visibility != Visibility.Visible)
				{
					ShowAnimated();
				}
			});
		}

		private void Activator_Deactivate()
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (Visibility == Visibility.Visible)
				{
					HideAnimated();
				}
			});
		}

		private void Activator_Toggle()
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (Visibility != Visibility.Visible)
				{
					ShowAnimated();
				}
				else
				{
					HideAnimated();
				}
			});
		}
	}
}
