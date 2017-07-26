/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Behaviour
{
	public interface IEventController
	{
		/// <summary>
		/// Wires up the event handling, i.e. subscribes to all relevant application events.
		/// </summary>
		void Start();

		/// <summary>
		/// Removes all event subscriptions.
		/// </summary>
		void Stop();
	}
}
