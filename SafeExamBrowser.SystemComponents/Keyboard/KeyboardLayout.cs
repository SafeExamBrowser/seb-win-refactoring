/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;

namespace SafeExamBrowser.SystemComponents.Keyboard
{
	internal class KeyboardLayout : IKeyboardLayout
	{
		internal InputLanguage InputLanguage { get; set; }

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
			return $"'{CultureName}' [{CultureCode}, {LayoutName}]";
		}
	}
}
