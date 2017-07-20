/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Configuration
{
	public interface IWorkingArea
	{
		/// <summary>
		/// Sets the Windows working area to accommodate to the taskbar's dimensions.
		/// </summary>
		void InitializeFor(ITaskbar taskbar);

		/// <summary>
		/// Resets the Windows working area to its previous (initial) state.
		/// </summary>
		void Reset();
	}
}
