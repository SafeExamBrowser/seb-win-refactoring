/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Client.Operations.Events
{
	internal class ApplicationInitializationFailedEventArgs : ActionRequiredEventArgs
	{
		internal string DisplayName { get; }
		internal string ExecutableName { get; }
		internal FactoryResult Result { get; }

		internal ApplicationInitializationFailedEventArgs(string displayName, string executableName, FactoryResult result)
		{
			DisplayName = displayName;
			ExecutableName = executableName;
			Result = result;
		}
	}
}
