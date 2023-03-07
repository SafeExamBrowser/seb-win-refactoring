/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class InvalidDataMessageArgs : MessageEventArgs
	{
		internal InvalidDataMessageArgs(string uri)
		{
			Icon = MessageBoxIcon.Error;
			Message = TextKey.MessageBox_InvalidConfigurationData;
			MessagePlaceholders["%%URI%%"] = uri;
			Title = TextKey.MessageBox_InvalidConfigurationDataTitle;
		}
	}
}
