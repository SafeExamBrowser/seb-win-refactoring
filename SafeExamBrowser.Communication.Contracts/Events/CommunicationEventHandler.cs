/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The default handler for communication events of an interlocutor.
	/// </summary>
	public delegate void CommunicationEventHandler();

	/// <summary>
	/// The handler with parameter for communication events of an interlocutor.
	/// </summary>
	public delegate void CommunicationEventHandler<T>(T args) where T : CommunicationEventArgs;

	public static class CommunicationEventHandlerExtensions
	{
		/// <summary>
		/// Executes the event handler asynchronously, i.e. on a separate thread.
		/// </summary>
		public static async Task InvokeAsync(this CommunicationEventHandler handler)
		{
			await Task.Run(() => handler?.Invoke());
		}

		/// <summary>
		/// Executes the event handler asynchronously, i.e. on a separate thread.
		/// </summary>
		public static async Task InvokeAsync<T>(this CommunicationEventHandler<T> handler, T args) where T : CommunicationEventArgs
		{
			await Task.Run(() => handler?.Invoke(args));
		}
	}
}
