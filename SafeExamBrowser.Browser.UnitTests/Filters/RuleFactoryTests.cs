/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.UnitTests.Filters
{
	[TestClass]
	public class RuleFactoryTests
	{
		private RuleFactory sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new RuleFactory();
		}

		[TestMethod]
		public void MustCreateCorrectRules()
		{
			Assert.IsInstanceOfType(sut.CreateRule(FilterRuleType.Regex), typeof(RegexRule));
			Assert.IsInstanceOfType(sut.CreateRule(FilterRuleType.Simplified), typeof(SimplifiedRule));
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void MustNotAllowUnsupportedFilterType()
		{
			sut.CreateRule((FilterRuleType) (-1));
		}
	}
}
