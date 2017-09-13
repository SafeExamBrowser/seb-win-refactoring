/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.SystemComponents
{
	public interface IKeyboardLayout
	{
		/// <summary>
		/// The three-letter ISO code of the culture to which this keyboard layout is associated.
		/// </summary>
		string CultureCode { get; }

		/// <summary>
		/// The unique identifier of the keyboard layout.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Specifies whether this is the current keyboard layout.
		/// </summary>
		bool IsCurrent { get; }

		/// <summary>
		/// The name of this keyboard layout.
		/// </summary>
		string Name { get; }
	}
}
