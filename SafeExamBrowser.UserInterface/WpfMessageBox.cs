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
	public class WpfMessageBox : IMessageBox
	{
		public void Show(string message, string title, MessageBoxAction action = MessageBoxAction.Confirm, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			MessageBox.Show(message, title, ToButton(action), ToImage(icon));
		}

		private MessageBoxButton ToButton(MessageBoxAction action)
		{
			switch (action)
			{
				default:
					return MessageBoxButton.OK;
			}
		}

		private MessageBoxImage ToImage(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Warning:
					return MessageBoxImage.Warning;
				case MessageBoxIcon.Error:
					return MessageBoxImage.Error;
				default:
					return MessageBoxImage.Information;
			}
		}
	}
}
