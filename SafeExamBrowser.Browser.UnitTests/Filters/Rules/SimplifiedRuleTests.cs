/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Filters.Rules;

namespace SafeExamBrowser.Browser.UnitTests.Filters.Rules
{
	[TestClass]
	public class SimplifiedRuleTests
	{
		private SimplifiedRule sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new SimplifiedRule();
		}
	}
}
