/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.UnitTests.Filters
{
	[TestClass]
	public class RequestFilterTests
	{
		private RequestFilter sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new RequestFilter();
		}

		[TestMethod]
		public void MustProcessBlockRulesFirst()
		{
			var allow = new FilterRuleSettings { Expression = "*", Type = FilterType.Simplified, Result = FilterResult.Allow };
			var block = new FilterRuleSettings { Expression = "*", Type = FilterType.Simplified, Result = FilterResult.Block };

			sut.Load(allow);
			sut.Load(block);

			var result = sut.Process("safeexambrowser.org");

			Assert.AreEqual(FilterResult.Block, result);
		}

		[TestMethod]
		public void MustProcessAllowRulesSecond()
		{
			var allow = new FilterRuleSettings { Expression = "*", Type = FilterType.Simplified, Result = FilterResult.Allow };
			var block = new FilterRuleSettings { Expression = "xyz", Type = FilterType.Simplified, Result = FilterResult.Block };

			sut.Load(allow);
			sut.Load(block);

			var result = sut.Process("safeexambrowser.org");

			Assert.AreEqual(FilterResult.Allow, result);
		}

		[TestMethod]
		public void MustReturnDefault()
		{
			var allow = new FilterRuleSettings { Expression = "xyz", Type = FilterType.Simplified, Result = FilterResult.Allow };
			var block = new FilterRuleSettings { Expression = "xyz", Type = FilterType.Simplified, Result = FilterResult.Block };

			sut.Default = (FilterResult) (-1);
			sut.Load(allow);
			sut.Load(block);

			var result = sut.Process("safeexambrowser.org");

			Assert.AreEqual((FilterResult) (-1), result);
		}

		[TestMethod]
		public void MustReturnDefaultWithoutRules()
		{
			sut.Default = FilterResult.Allow;
			var result = sut.Process("safeexambrowser.org");
			Assert.AreEqual(FilterResult.Allow, result);

			sut.Default = FilterResult.Block;
			result = sut.Process("safeexambrowser.org");
			Assert.AreEqual(FilterResult.Block, result);
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void MustNotAllowUnsupportedResult()
		{
			sut.Load(new FilterRuleSettings { Result = (FilterResult) (-1) });
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void MustNotAllowUnsupportedFilterType()
		{
			sut.Load(new FilterRuleSettings { Type = (FilterType) (-1) });
		}
	}
}
