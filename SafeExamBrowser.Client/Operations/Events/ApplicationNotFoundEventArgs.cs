/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Client.Operations.Events
{
	internal class ApplicationNotFoundEventArgs : ActionRequiredEventArgs
	{
		internal string CustomPath { get; set; }
		internal string DisplayName { get; }
		internal string ExecutableName { get; }
		internal bool Success { get; set; }

		internal ApplicationNotFoundEventArgs(string displayName, string executableName)
		{
			DisplayName = displayName;
			ExecutableName = executableName;
		}
	}
}
