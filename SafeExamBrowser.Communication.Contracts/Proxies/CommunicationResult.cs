/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// Defines the result of a communication between an <see cref="ICommunicationProxy"/> and its <see cref="ICommunicationHost"/>.
	/// </summary>
	public class CommunicationResult
	{
		/// <summary>
		/// Defines whether the communication was successful or not.
		/// </summary>
		public bool Success { get; protected set; }

		public CommunicationResult(bool success)
		{
			Success = success;
		}
	}

	/// <summary>
	/// Defines the result of a communication between an <see cref="ICommunicationProxy"/> and its <see cref="ICommunicationHost"/>.
	/// </summary>
	/// <typeparam name="T">The type of the expected response value.</typeparam>
	public class CommunicationResult<T> : CommunicationResult
	{
		/// <summary>
		/// The response value. Can be <c>null</c> or <c>default(T)</c> in case the communication has failed!
		/// </summary>
		public T Value { get; protected set; }

		public CommunicationResult(bool success, T value) : base(success)
		{
			Value = value;
		}
	}
}
