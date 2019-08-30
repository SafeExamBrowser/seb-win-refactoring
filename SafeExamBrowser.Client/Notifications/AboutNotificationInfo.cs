/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationInfo : INotificationInfo
	{
		private IText text;

		public string Tooltip => text.Get(TextKey.Notification_AboutTooltip);
		public IIconResource IconResource { get; } = new AboutNotificationIconResource();

		public AboutNotificationInfo(IText text)
		{
			this.text = text;
		}
	}
}
