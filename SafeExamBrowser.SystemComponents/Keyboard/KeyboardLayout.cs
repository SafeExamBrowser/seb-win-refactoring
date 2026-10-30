/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.SystemComponents.Keyboard
{
	internal class KeyboardLayout : IKeyboardLayout
	{
		internal IInputProfile Profile { get; set; }

		public string CultureCode { get; set; }
		public string CultureName { get; set; }
		public Guid Id { get; }
		public bool IsCurrent { get; set; }
		public string LayoutName { get; set; }

		public KeyboardLayout()
		{
			Id = Guid.NewGuid();
		}

		public override string ToString()
		{
			return $"'{CultureName}' ({CultureCode}, {LayoutName}, Profile: {Profile})";
		}
	}
}
