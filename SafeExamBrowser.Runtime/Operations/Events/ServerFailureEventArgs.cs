/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class ServerFailureEventArgs : ActionRequiredEventArgs
	{
		public bool Abort { get; set; }
		public bool Fallback { get; set; }
		public string Message { get; set; }
		public bool Retry { get; set; }
		public bool ShowFallback { get; }

		public ServerFailureEventArgs(string message, bool showFallback)
		{
			Message = message;
			ShowFallback = showFallback;
		}
	}
}
