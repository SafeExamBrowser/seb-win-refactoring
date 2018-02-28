/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.WindowsApi
{
	public class Desktop : IDesktop
	{
		private ILogger logger;

		// TODO: Implement desktop functionality!
		public string CurrentName => "Default";

		public Desktop(ILogger logger)
		{
			this.logger = logger;
		}
	}
}
