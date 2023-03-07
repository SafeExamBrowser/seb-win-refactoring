/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class MessageEventArgs : ActionRequiredEventArgs
	{
		internal MessageBoxAction Action { get; set; }
		internal MessageBoxIcon Icon { get; set; }
		internal TextKey Message { get; set; }
		internal MessageBoxResult Result { get; set; }
		internal TextKey Title { get; set; }
		internal Dictionary<string, string> MessagePlaceholders { get; private set; }
		internal Dictionary<string, string> TitlePlaceholders { get; private set; }

		public MessageEventArgs()
		{
			Action = MessageBoxAction.Ok;
			MessagePlaceholders = new Dictionary<string, string>();
			TitlePlaceholders = new Dictionary<string, string>();
		}
	}
}
