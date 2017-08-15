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
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class PowerSupplyControl : UserControl, ISystemPowerSupplyControl
	{
		public PowerSupplyControl()
		{
			InitializeComponent();
			InitializePowerSupplyControl();
		}

		public void SetBatteryCharge(double? percentage)
		{
			throw new NotImplementedException();
		}

		public void SetPowerGridConnection(bool connected)
		{
			throw new NotImplementedException();
		}

		public void SetTooltip(string text)
		{
			throw new NotImplementedException();
		}

		private void InitializePowerSupplyControl()
		{
			Button.Resources.MergedDictionaries.Add(new ResourceDictionary
			{
				Source = (new Uri("/SafeExamBrowser.UserInterface;component/Styles/ButtonStyles.xaml", UriKind.RelativeOrAbsolute))
			});
		}
	}
}
