/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Network.Contracts;
using SafeExamBrowser.Network.Contracts.Filter;

namespace SafeExamBrowser.Network.Filter
{
	public class RequestFilter : IRequestFilter
	{
		public FilterResult Default { private get; set; }

		public void Load(FilterRule rule)
		{
			throw new NotImplementedException();
		}

		public FilterResult Process(Request request)
		{
			throw new NotImplementedException();
		}
	}
}
