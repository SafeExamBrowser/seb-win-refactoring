/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.Notifications;
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
		/// The notifications for all active proctoring providers.
		/// </summary>
		IEnumerable<INotification> Notifications { get; }

		/// <summary>
		/// Fired when the hand has been lowered.
		/// </summary>
		event ProctoringEventHandler HandLowered;

		/// <summary>
		/// Fired when the hand has been raised.
		/// </summary>
		event ProctoringEventHandler HandRaised;

		/// <summary>
		/// Event fired when the status of the remaining work has been updated.
		/// </summary>
		event RemainingWorkUpdatedEventHandler RemainingWorkUpdated;

		/// <summary>
		/// Executes any remaining work like e.g. the transmission of cached screen shots. Make sure to do so before calling <see cref="Terminate"/>.
		/// </summary>
		void ExecuteRemainingWork();

		/// <summary>
		/// Indicates whether there is any remaining work which needs to be done before the proctoring can be terminated.
		/// </summary>
		bool HasRemainingWork();

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
		void RaiseHand(string message = default);

		/// <summary>
		/// Stops the proctoring functionality. Make sure to call <see cref="ExecuteRemainingWork"/> beforehand if necessary.
		/// </summary>
		void Terminate();
	}
}
