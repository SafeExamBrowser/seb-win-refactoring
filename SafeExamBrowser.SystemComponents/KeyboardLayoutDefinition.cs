/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.SystemComponents
{
	internal class KeyboardLayoutDefinition : IKeyboardLayout
	{
		internal InputLanguage Language { get; set; }

		public string CultureCode { get; set; }
		public Guid Id { get; }
		public bool IsCurrent { get; set; }
		public string Name { get; set; }

		public KeyboardLayoutDefinition()
		{
			Id = Guid.NewGuid();
		}
	}
}
