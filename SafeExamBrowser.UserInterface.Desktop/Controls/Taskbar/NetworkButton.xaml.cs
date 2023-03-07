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
using SafeExamBrowser.SystemComponents.Contracts.Network;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.Taskbar
{
	internal partial class NetworkButton : UserControl
	{
		private readonly IWirelessNetwork network;

		internal event EventHandler NetworkSelected;

		internal NetworkButton(IWirelessNetwork network)
		{
			this.network = network;

			InitializeComponent();
			InitializeNetworkButton();
		}

		private void InitializeNetworkButton()
		{
			Button.Click += (o, args) => NetworkSelected?.Invoke(this, EventArgs.Empty);
			IsCurrentTextBlock.Visibility = network.Status == ConnectionStatus.Connected ? Visibility.Visible : Visibility.Hidden;
			NetworkNameTextBlock.Text = network.Name;
			SignalStrengthTextBlock.Text = $"{network.SignalStrength}%";
		}

		public void SetFocus()
		{
			Button.Focus();
		}
	}
}
