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
	/// Defines the parameters of a proctoring instruction for provider Zoom.
	/// </summary>
	public class ZoomInstruction : InstructionEventArgs
	{
		public string MeetingNumber { get; set; }
		public string Password { get; set; }
		public string SdkKey { get; set; }
		public string Signature { get; set; }
		public string Subject { get; set; }
		public string UserName { get; set; }
	}
}
