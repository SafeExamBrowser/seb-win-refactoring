/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Data
{
	internal class MetaData
	{
		internal string ApplicationInfo { get; set; }
		internal string BrowserInfo { get; set; }
		internal TimeSpan Elapsed { get; set; }
		internal string TriggerInfo { get; set; }
		internal string Urls { get; set; }
		internal string WindowTitle { get; set; }

		internal string ToJson()
		{
			var json = new JObject
			{
				[Header.Metadata.ApplicationInfo] = ApplicationInfo,
				[Header.Metadata.BrowserInfo] = BrowserInfo,
				[Header.Metadata.BrowserUrls] = Urls,
				[Header.Metadata.TriggerInfo] = TriggerInfo,
				[Header.Metadata.WindowTitle] = WindowTitle
			};

			return json.ToString(Formatting.None);
		}
	}
}
