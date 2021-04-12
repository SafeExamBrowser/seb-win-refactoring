/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Data
{
	internal class Attributes
	{
		public bool AllowChat { get; set; }
		public bool ReceiveAudio { get; set; }
		public bool ReceiveVideo { get; set; }
		public string RoomName { get; set; }
		public string ServerUrl { get; set; }
		public string Token { get; set; }
	}
}
