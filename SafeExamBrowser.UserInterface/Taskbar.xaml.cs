/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class Taskbar : Window, ITaskbar
	{
		public Taskbar()
		{
			InitializeComponent();
		}

		public void AddButton(ITaskbarButton button)
		{
			if (button is UIElement)
			{
				ApplicationAreaStackPanel.Children.Add(button as UIElement);
			}
		}

		public void SetPosition(int x, int y)
		{
			Left = x;
			Top = y;
		}

		public void SetSize(int widht, int height)
		{
			Width = widht;
			Height = height;
		}
	}
}
