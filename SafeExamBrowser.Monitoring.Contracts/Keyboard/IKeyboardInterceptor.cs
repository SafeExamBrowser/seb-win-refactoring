/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Contracts.Keyboard
{
	/// <summary>
	/// Intercepts all keyboard input (except the Secure Attention Sequence: https://en.wikipedia.org/wiki/Secure_attention_key).
	/// </summary>
	public interface IKeyboardInterceptor
	{
		/// <summary>
		/// Starts intercepting keyboard input.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops intercepting keyboard input.
		/// </summary>
		void Stop();
	}
}
