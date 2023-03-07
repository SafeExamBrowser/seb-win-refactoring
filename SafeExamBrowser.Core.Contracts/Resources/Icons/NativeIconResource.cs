/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Core.Contracts.Resources.Icons
{
	/// <summary>
	/// Defines an icon resource which is managed by the operating system.
	/// </summary>
	public class NativeIconResource : IconResource
	{
		/// <summary>
		/// The handle of the icon.
		/// </summary>
		public IntPtr Handle { get; set; }
	}
}
