/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.UnitTests.Filters.Rules
{
	[TestClass]
	public class RegexRuleTests
	{
		private RegexRule sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new RegexRule();
		}

		[TestMethod]
		public void MustIgnoreCase()
		{
			sut.Initialize(new FilterRuleSettings { Expression = Regex.Escape("http://www.test.org/path/file.txt?param=123") });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));

			sut.Initialize(new FilterRuleSettings { Expression = Regex.Escape("HTTP://WWW.TEST.ORG/PATH/FILE.TXT?PARAM=123") });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));
		}

		[TestMethod]
		public void MustInitializeResult()
		{
			foreach (var result in Enum.GetValues(typeof(FilterResult)))
			{
				sut.Initialize(new FilterRuleSettings { Expression = "", Result = (FilterResult) result });
				Assert.AreEqual(result, sut.Result);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowUndefinedExpression()
		{
			sut.Initialize(new FilterRuleSettings());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustValidateExpression()
		{
			sut.Initialize(new FilterRuleSettings { Expression = "ç+\"}%&*/(+)=?{=*+¦]@#°§]`?´^¨'°[¬|¢" });
		}
	}
}
