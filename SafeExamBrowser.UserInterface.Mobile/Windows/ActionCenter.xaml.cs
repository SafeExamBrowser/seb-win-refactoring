/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class ActionCenter : Window, IActionCenter
	{
		public bool ShowClock
		{
			set { Dispatcher.Invoke(() => Clock.Visibility = value ? Visibility.Visible : Visibility.Collapsed); }
		}

		public bool ShowQuitButton
		{
			set { Dispatcher.Invoke(() => QuitButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed); }
		}

		public event QuitButtonClickedEventHandler QuitButtonClicked;

		internal ActionCenter()
		{
			InitializeComponent();
			InitializeActionCenter();
		}

		public void AddApplicationControl(IApplicationControl control, bool atFirstPosition = false)
		{
			if (control is UIElement uiElement)
			{
				if (atFirstPosition)
				{
					ApplicationPanel.Children.Insert(0, uiElement);
				}
				else
				{
					ApplicationPanel.Children.Add(uiElement);
				}
			}
		}

		public void AddNotificationControl(INotificationControl control)
		{
			if (control is UIElement uiElement)
			{
				ControlPanel.Children.Insert(ControlPanel.Children.Count - 2, uiElement);
			}
		}

		public void AddSystemControl(ISystemControl control)
		{
			if (control is UIElement uiElement)
			{
				ControlPanel.Children.Insert(ControlPanel.Children.Count - 2, uiElement);
			}
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
		}

		public new void Hide()
		{
			Dispatcher.Invoke(HideAnimated);
		}

		public void InitializeBounds()
		{
			Dispatcher.Invoke(() =>
			{
				Height = SystemParameters.WorkArea.Height;
				Top = 0;
				Left = -Width;
			});
		}

		public void InitializeText(IText text)
		{
			QuitButton.ToolTip = text.Get(TextKey.Shell_QuitButton);
			QuitButton.Text.Text = text.Get(TextKey.Shell_QuitButton);
		}

		public void Promote()
		{
			Task.Run(() =>
			{
				Dispatcher.Invoke(ShowAnimated);
				Thread.Sleep(2000);
				Dispatcher.Invoke(HideAnimated);
			});
		}

		public void Register(IActionCenterActivator activator)
		{
			activator.Activated += Activator_Activated;
			activator.Deactivated += Activator_Deactivated;
			activator.Toggled += Activator_Toggled;
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
			storyboard.Completed += HideAnimation_Completed;
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

			storyboard.Children.Add(animation);
			storyboard.Completed += ShowAnimation_Completed;
			storyboard.Begin();
		}

		private void ShowAnimation_Completed(object sender, EventArgs e)
		{
			Activate();
			Deactivated += ActionCenter_Deactivated;
		}

		private void HideAnimation_Completed(object sender, EventArgs e)
		{
			Deactivated -= ActionCenter_Deactivated;
			base.Hide();
		}

		private void ActionCenter_Deactivated(object sender, EventArgs e)
		{
			HideAnimated();
		}

		private void Activator_Activated()
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (Visibility != Visibility.Visible)
				{
					ShowAnimated();
				}
			});
		}

		private void Activator_Deactivated()
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (Visibility == Visibility.Visible)
				{
					HideAnimated();
				}
			});
		}

		private void Activator_Toggled()
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

		private void InitializeActionCenter()
		{
			QuitButton.Clicked += (args) => QuitButtonClicked?.Invoke(args);
		}
	}
}
