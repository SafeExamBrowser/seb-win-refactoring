/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Configuration
{
	public class Session : ISession
	{
		public IProcess ClientProcess { get; set; }
		public Guid Id { get; set; }
		public Guid StartupToken { get; set; }
	}
}
