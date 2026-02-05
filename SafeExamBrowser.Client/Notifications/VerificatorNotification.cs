/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Integrity.Contracts;

namespace SafeExamBrowser.Client.Notifications
{
	internal class VerificatorNotification : INotification
	{
		private readonly IText text;
		private readonly IVerificator verificator;

		public bool CanActivate { get; }
		public string Tooltip { get; }
		public IconResource IconResource { get; }

		public event NotificationChangedEventHandler NotificationChanged { add { } remove { } }

		internal VerificatorNotification(IText text, IVerificator verificator)
		{
			this.text = text;
			this.verificator = verificator;

			CanActivate = true;
			// TODO: Load icon once available!
			// IconResource = new BitmapIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/VerificatorNotification.ico") };

			// TODO: Properly load text (see reference in issue)!
			Tooltip = "Show QR Code for Verificator App";
		}

		public void Activate()
		{
			verificator.Activate();
		}

		public void Terminate()
		{
			verificator.Deactivate();
		}
	}
}
