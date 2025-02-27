/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Contracts.Events.Proctoring
{
	/// <summary>
	/// Defines the parameters of a proctoring instruction for provider Jitsi Meet.
	/// </summary>
	public class JitsiMeetInstruction : InstructionEventArgs
	{
		public string RoomName { get; set; }
		public string ServerUrl { get; set; }
		public string Token { get; set; }
	}
}
