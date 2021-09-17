/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
{
	public partial class RaiseHandControl : UserControl, INotificationControl
	{
		private readonly IProctoringController controller;
		private readonly ProctoringSettings settings;
		private readonly IText text;

		public RaiseHandControl(IProctoringController controller, ProctoringSettings settings, IText text)
		{
			this.controller = controller;
			this.settings = settings;
			this.text = text;

			InitializeComponent();
			InitializeRaiseHandControl();
		}

		private void InitializeRaiseHandControl()
		{
			var originalBrush = NotificationButton.Background;

			controller.HandLowered += () => Dispatcher.Invoke(ShowLowered);
			controller.HandRaised += () => Dispatcher.Invoke(ShowRaised);

			HandButton.Click += RaiseHandButton_Click;
			HandButtonText.Text = text.Get(TextKey.Notification_ProctoringRaiseHand);

			NotificationButton.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			NotificationButton.PreviewMouseLeftButtonUp += NotificationButton_PreviewMouseLeftButtonUp;
			NotificationButton.PreviewMouseRightButtonUp += NotificationButton_PreviewMouseRightButtonUp;
			NotificationButton.ToolTip = text.Get(TextKey.Notification_ProctoringHandLowered);

			Popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));
			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				NotificationButton.Background = Brushes.LightGray;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				NotificationButton.Background = originalBrush;
			};
		}

		private void NotificationButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (settings.ForceRaiseHandMessage || Popup.IsOpen)
			{
				Popup.IsOpen = !Popup.IsOpen;
			}
			else
			{
				ToggleHand();
			}
		}

		private void NotificationButton_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			Popup.IsOpen = !Popup.IsOpen;
		}

		private CustomPopupPlacement[] Popup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			return new[]
			{
				new CustomPopupPlacement(new Point(targetSize.Width / 2 - popupSize.Width / 2, -popupSize.Height), PopupPrimaryAxis.None)
			};
		}

		private void RaiseHandButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleHand();
		}

		private void ToggleHand()
		{
			if (controller.IsHandRaised)
			{
				controller.LowerHand();
			}
			else
			{
				controller.RaiseHand(Message.Text);
				Message.Clear();
			}
		}

		private void ShowLowered()
		{
			HandButtonText.Text = text.Get(TextKey.Notification_ProctoringRaiseHand);
			NotificationButton.ToolTip = text.Get(TextKey.Notification_ProctoringHandLowered);
			TextBlock.Text = "L";
		}

		private void ShowRaised()
		{
			HandButtonText.Text = text.Get(TextKey.Notification_ProctoringLowerHand);
			NotificationButton.ToolTip = text.Get(TextKey.Notification_ProctoringHandRaised);
			TextBlock.Text = "R";
		}
	}
}
