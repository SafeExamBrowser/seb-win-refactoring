/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// The verificator overlay displays verification codes for the respective mobile phone application.
	/// </summary>
	public interface IVerificatorOverlay : IWindow
	{
		/// <summary>
		/// Updates the verification code display with the given image data.
		/// </summary>
		void UpdateCode(byte[] data);
	}
}
