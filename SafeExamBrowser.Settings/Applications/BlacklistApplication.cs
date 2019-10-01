/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Applications
{
	/// <summary>
	/// TODO
	/// </summary>
	[Serializable]
	public class BlacklistApplication
	{
		/// <summary>
		/// 
		/// </summary>
		public bool AutoTerminate { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ExecutableName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ExecutableOriginalName { get; set; }
	}
}
