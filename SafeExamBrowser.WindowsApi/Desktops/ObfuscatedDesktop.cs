/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Desktops
{
	internal class ObfuscatedDesktop : Desktop
	{
		private readonly IntPtr obfuscatedHandle;
		private readonly string obfuscatedName;

		public ObfuscatedDesktop(IntPtr handle, string name, IntPtr obfuscatedHandle, string obfuscatedName) : base(handle, name)
		{
			this.obfuscatedHandle = obfuscatedHandle;
			this.obfuscatedName = obfuscatedName;
		}

		public override string ToString()
		{
			return $"'{obfuscatedName}' [{obfuscatedHandle}]";
		}
	}
}
