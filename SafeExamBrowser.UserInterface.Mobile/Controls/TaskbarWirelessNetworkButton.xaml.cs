/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class TaskbarWirelessNetworkButton : UserControl
	{
		private readonly IWirelessNetwork network;

		public bool IsCurrent
		{
			set { IsCurrentTextBlock.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		public string NetworkName
		{
			set { NetworkNameTextBlock.Text = value; }
		}

		public int SignalStrength
		{
			set { SignalStrengthTextBlock.Text = $"{value}%"; }
		}

		public event WirelessNetworkSelectedEventHandler NetworkSelected;

		public TaskbarWirelessNetworkButton(IWirelessNetwork network)
		{
			this.network = network;

			InitializeComponent();
			InitializeEvents();
		}

		private void InitializeEvents()
		{
			Button.Click += (o, args) => NetworkSelected?.Invoke(network.Id);
		}
	}
}
