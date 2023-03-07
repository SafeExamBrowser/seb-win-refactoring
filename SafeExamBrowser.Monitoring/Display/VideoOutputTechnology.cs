/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Display
{
	/// <remarks>
	/// See https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/d3dkmdt/ne-d3dkmdt-_d3dkmdt_video_output_technology
	/// </remarks>
	internal enum VideoOutputTechnology : long
	{
		Uninitialized = -2,
		Other = -1,
		HD15 = 0,
		SVideo = 1,
		CompositeVideo = 2,
		ComponentVideo = 3,
		DVI = 4,
		HDMI = 5,
		LVDS = 6,
		DJPN = 8,
		SDI = 9,
		DisplayPortExternal = 10,
		DisplayPortEmbedded = 11,
		UDIExternal = 12,
		UDIEmbedded = 13,
		SDTVDongle = 14,
		MiraCast = 15,
		Internal = 0x80000000,
		SVideo4Pin = SVideo,
		SVideo7Pin = SVideo,
		RF = ComponentVideo,
		RCA3Component = ComponentVideo,
		BNC = ComponentVideo
	}
}
