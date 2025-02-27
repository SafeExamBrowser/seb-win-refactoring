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
	/// Defines the parameters of a proctoring instruction for the screen proctoring implementation.
	/// </summary>
	public class ScreenProctoringInstruction : InstructionEventArgs
	{
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string EncryptionSecret { get; set; }
		public string GroupId { get; set; }
		public string ServiceUrl { get; set; }
		public string SessionId { get; set; }
	}
}
