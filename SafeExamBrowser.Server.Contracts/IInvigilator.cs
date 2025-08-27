/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Server.Contracts.Events;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Contracts
{
	/// <summary>
	/// Provides invigilation functionality for server sessions.
	/// </summary>
	public interface IInvigilator
	{
		/// <summary>
		/// Indicates whether the hand is currently raised.
		/// </summary>
		bool IsHandRaised { get; }

		/// <summary>
		/// Fired when the hand has been lowered.
		/// </summary>
		event InvigilationEventHandler HandLowered;

		/// <summary>
		/// Fired when the hand has been raised.
		/// </summary>
		event InvigilationEventHandler HandRaised;

		/// <summary>
		/// Initializes the invigilation functionality according to the given settings.
		/// </summary>
		void Initialize(InvigilationSettings settings);

		/// <summary>
		/// Lowers the hand.
		/// </summary>
		void LowerHand();

		/// <summary>
		/// Raises the hand, optionally with the given message.
		/// </summary>
		void RaiseHand(string message = default);
	}
}
