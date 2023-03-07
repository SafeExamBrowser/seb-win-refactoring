/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Contracts.Events
{
	/// <summary>
	/// Defines all parameters for a proctoring instruction received by the <see cref="IServerProxy"/>.
	/// </summary>
	public class ProctoringInstructionEventArgs
	{
		public string JitsiMeetRoomName { get; set; }
		public string JitsiMeetServerUrl { get; set; }
		public string JitsiMeetToken { get; set; }
		public string ZoomMeetingNumber { get; set; }
		public string ZoomPassword { get; set; }
		public string ZoomSdkKey { get; set; }
		public string ZoomSignature { get; set; }
		public string ZoomSubject { get; set; }
		public string ZoomUserName { get; set; }
	}
}
