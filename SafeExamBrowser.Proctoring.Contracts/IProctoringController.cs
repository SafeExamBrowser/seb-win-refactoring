/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.Contracts
{
	/// <summary>
	/// Defines the remote proctoring functionality.
	/// </summary>
	public interface IProctoringController
	{
		/// <summary>
		/// Initializes the given settings and starts the proctoring if the settings are valid.
		/// </summary>
		void Initialize(ProctoringSettings settings);

		/// <summary>
		/// Stops the proctoring functionality.
		/// </summary>
		void Terminate();
	}
}
