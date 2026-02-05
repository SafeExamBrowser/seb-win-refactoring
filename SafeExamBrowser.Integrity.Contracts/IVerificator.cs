/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Integrity.Contracts
{
	/// <summary>
	/// Provides functionality related to application verification.
	/// </summary>
	public interface IVerificator
	{
		/// <summary>
		/// Activates the verification code generation.
		/// </summary>
		void Activate();

		/// <summary>
		/// Deactivates the verification code generation.
		/// </summary>
		void Deactivate();

		/// <summary>
		/// Registers the specified activator for the verificator.
		/// </summary>
		void Register(IVerificatorActivator activator);
	}
}
