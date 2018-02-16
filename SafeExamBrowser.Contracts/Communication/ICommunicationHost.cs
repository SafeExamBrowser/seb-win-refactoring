/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Communication
{
	public delegate void CommunicationEventHandler();

	public interface ICommunicationHost
	{
		/// <summary>
		/// Indicates whether the host is running and ready for communication.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Starts the host and opens it for communication.
		/// </summary>
		/// <exception cref="System.ServiceModel.CommunicationException">If the host fails to start.</exception>
		void Start();

		/// <summary>
		/// Closes and terminates the host.
		/// </summary>
		/// <exception cref="System.ServiceModel.CommunicationException">If the host fails to terminate.</exception>
		void Stop();
	}
}
