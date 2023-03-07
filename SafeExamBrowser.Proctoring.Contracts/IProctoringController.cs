/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.Contracts
{
	/// <summary>
	/// Defines the remote proctoring functionality.
	/// </summary>
	public interface IProctoringController
	{
		/// <summary>
		/// Indicates whether the hand is currently raised.
		/// </summary>
		bool IsHandRaised { get; }

		/// <summary>
		/// Fired when the hand has been lowered.
		/// </summary>
		event ProctoringEventHandler HandLowered;

		/// <summary>
		/// Fired when the hand has been raised.
		/// </summary>
		event ProctoringEventHandler HandRaised;

		/// <summary>
		/// Initializes the given settings and starts the proctoring if the settings are valid.
		/// </summary>
		void Initialize(ProctoringSettings settings);

		/// <summary>
		/// Lowers the hand.
		/// </summary>
		void LowerHand();

		/// <summary>
		/// Raises the hand, optionally with the given message.
		/// </summary>
		void RaiseHand(string message = default(string));

		/// <summary>
		/// Stops the proctoring functionality.
		/// </summary>
		void Terminate();
	}
}
