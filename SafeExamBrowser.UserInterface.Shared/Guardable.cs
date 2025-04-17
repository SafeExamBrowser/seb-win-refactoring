/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.UserInterface.Shared
{
	public abstract class Guardable
	{
		private readonly IWindowGuard windowGuard;

		public Guardable(IWindowGuard windowGuard = default)
		{
			this.windowGuard = windowGuard;
		}

		protected T Guard<T>(T window)
		{
			windowGuard?.Register(window);

			return window;
		}
	}
}
