/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.I18n;

namespace SafeExamBrowser.Client.Notifications
{
	internal class LogNotificationInfo : INotificationInfo
	{
		public string Tooltip { get; private set; }
		public IIconResource IconResource { get; private set; }

		public LogNotificationInfo(IText text)
		{
			Tooltip = text.Get(TextKey.Notification_LogTooltip);
			IconResource = new LogNotificationIconResource();
		}
	}
}
