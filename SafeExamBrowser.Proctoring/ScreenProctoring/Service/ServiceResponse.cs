/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service
{
	internal class ServiceResponse
	{
		internal string Message { get; }
		internal bool Success { get; }

		internal ServiceResponse(bool success, string message = default)
		{
			Message = message;
			Success = success;
		}
	}

	internal class ServiceResponse<T> : ServiceResponse
	{
		internal T Value { get; }

		internal ServiceResponse(bool success, T value, string message = default) : base(success, message)
		{
			Value = value;
		}
	}
}
