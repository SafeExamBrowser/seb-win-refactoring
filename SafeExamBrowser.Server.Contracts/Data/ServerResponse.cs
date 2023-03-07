/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Contracts.Data
{
	/// <summary>
	/// Defines the result of a communication with a SEB server.
	/// </summary>
	public class ServerResponse
	{
		/// <summary>
		/// The message retrieved by the server in case the communication failed.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Defines whether the communication was successful or not.
		/// </summary>
		public bool Success { get; }

		public ServerResponse(bool success, string message = default(string))
		{
			Message = message;
			Success = success;
		}
	}

	/// <summary>
	/// Defines the result of a communication with a SEB server.
	/// </summary>
	/// <typeparam name="T">The type of the expected response value.</typeparam>
	public class ServerResponse<T> : ServerResponse
	{
		/// <summary>
		/// The response value. Can be <c>null</c> or <c>default(T)</c> in case the communication failed!
		/// </summary>
		public T Value { get; }

		public ServerResponse(bool success, T value, string message = default(string)) : base(success, message)
		{
			Value = value;
		}
	}
}
