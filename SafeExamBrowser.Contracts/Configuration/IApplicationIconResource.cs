/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	public interface IApplicationIconResource
	{
		/// <summary>
		/// The <c>Uri</c> pointing to the application icon.
		/// </summary>
		Uri Uri { get; }

		/// <summary>
		/// Indicates whether the icon resource consists of a <c>Uri</c>.
		/// </summary>
		bool IsUriResource { get; }
	}
}
