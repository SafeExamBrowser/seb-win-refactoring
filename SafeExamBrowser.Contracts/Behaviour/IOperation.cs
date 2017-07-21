/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Behaviour
{
	public interface IOperation
	{
		/// <summary>
		/// The splash screen to be used to show status information to the user.
		/// </summary>
		ISplashScreen SplashScreen { set; }

		/// <summary>
		/// Performs the operation.
		/// </summary>
		void Perform();

		/// <summary>
		/// Reverts all changes which were made when performing the operation.
		/// </summary>
		void Revert();
	}
}
