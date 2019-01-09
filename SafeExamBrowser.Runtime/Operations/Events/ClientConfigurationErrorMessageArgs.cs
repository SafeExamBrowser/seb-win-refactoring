/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class ClientConfigurationErrorMessageArgs : MessageEventArgs
	{
		internal ClientConfigurationErrorMessageArgs()
		{
			Icon = MessageBoxIcon.Error;
			Message = TextKey.MessageBox_ClientConfigurationError;
			Title = TextKey.MessageBox_ClientConfigurationErrorTitle;
		}
	}
}
