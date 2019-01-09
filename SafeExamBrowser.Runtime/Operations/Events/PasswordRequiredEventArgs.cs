/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class PasswordRequiredEventArgs : ActionRequiredEventArgs
	{
		public string Password { get; set; }
		public PasswordRequestPurpose Purpose { get; set; }
		public bool Success { get; set; }
	}
}
