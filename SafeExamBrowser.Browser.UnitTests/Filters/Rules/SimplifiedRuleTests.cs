/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser;

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

		[TestMethod]
		public void TestAlphanumericExpressionAsHost()
		{
			var expression = "hostname-123";
			var positive = new[]
			{
				$"scheme://{expression}.org",
				$"scheme://www.{expression}.org",
				$"scheme://subdomain.{expression}.com",
				$"scheme://subdomain-1.subdomain-2.{expression}.org",
				$"scheme://user:password@www.{expression}.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				$"scheme://hostname.org",
				$"scheme://hostname-12.org",
				$"scheme://{expression}4.org",
				$"scheme://{expression}.realhost.org",
				$"scheme://subdomain.{expression}.realhost.org",
				$"scheme://www.realhost.{expression}",
				$"{expression}://www.host.org",
				$"scheme://www.host.org/{expression}/path",
				$"scheme://www.host.org/path?param={expression}",
				$"scheme://{expression}:password@www.host.org",
				$"scheme://user:{expression}@www.host.org",
				$"scheme://user:password@www.host.org/path?param=123#{expression}"
			};

			Execute(expression, positive, negative, false);
		}

		[TestMethod]
		public void TestHostExpressionWithDomain()
		{
			var expression = "123-hostname.org";
			var positive = new[]
			{
				$"scheme://{expression}",
				$"scheme://www.{expression}",
				$"scheme://subdomain.{expression}",
				$"scheme://subdomain-1.subdomain-2.{expression}",
				$"scheme://user:password@www.{expression}/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				$"scheme://123.org",
				$"scheme://123-host.org",
				$"scheme://{expression}.com",
				$"scheme://{expression}s.org",
				$"scheme://{expression}.realhost.org",
				$"scheme://subdomain.{expression}.realhost.org",
				$"scheme{expression}://www.host.org",
				$"scheme://www.host.org/{expression}/path",
				$"scheme://www.host.org/path?param={expression}",
				$"scheme://{expression}:password@www.host.org",
				$"scheme://user:{expression}@www.host.org",
				$"scheme://user:password@www.host.org/path?param=123#{expression}"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostExpressionWithWildcard()
		{
			var expression = "test.*.org";
			var positive = new[]
			{
				"scheme://test.host.org",
				"scheme://test.host.domain.org",
				"scheme://subdomain.test.host.org",
				"scheme://user:password@test.domain.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://test.org",
				"scheme://host.com/test.host.org",
				"scheme://www.host.org/test.host.org/path",
				"scheme://www.host.org/path?param=test.host.org",
				"scheme://test.host.org:password@www.host.org",
				"scheme://user:test.host.org@www.host.org",
				"scheme://user:password@www.host.org/path?param=123#test.host.org"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostExpressionWithWildcardAsSuffix()
		{
			var expression = "test.host.*";
			var positive = new[]
			{
				"scheme://test.host.org",
				"scheme://test.host.domain.org",
				"scheme://subdomain.test.host.org",
				"scheme://user:password@test.host.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.com",
				"scheme://test.host",
				"scheme://host.com/test.host.txt"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostExpressionWithWildcardAsPrefix()
		{
			var expression = "*.org";
			var positive = new[]
			{
				"scheme://domain.org",
				"scheme://test.host.org",
				"scheme://test.host.domain.org",
				"scheme://user:password@www.host.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://org",
				"scheme://host.com",
				"scheme://test.net",
				"scheme://test.ch",
				"scheme://host.com/test.org"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostExpressionWithExactSubdomain()
		{
			var expression = ".www.host.org";
			var positive = new[]
			{
				"scheme://www.host.org",
				"scheme://user:password@www.host.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://test.www.host.org",
				"scheme://www.host.org.com"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void TestExpressionWithPortNumber()
		{
			var expression = "host.org:2020";
			var positive = new[]
			{
				"scheme://host.org:2020",
				"scheme://www.host.org:2020",
				"scheme://user:password@www.host.org:2020/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://www.host.org",
				"scheme://www.host.org:2",
				"scheme://www.host.org:20",
				"scheme://www.host.org:202",
				"scheme://www.host.org:20202"
			};

			Execute(expression, positive, negative);
		}

		[TestMethod]
		public void MustIgnoreCase()
		{
			sut.Initialize(new FilterRuleSettings { Expression = "http://www.test.org/path/file.txt?param=123" });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));

			sut.Initialize(new FilterRuleSettings { Expression = "HTTP://WWW.TEST.ORG/PATH/FILE.TXT?PARAM=123" });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));
		}

		// TODO
		//[TestMethod]
		//public void MustIgnoreTrailingSlash()
		//{
		//	Assert.Fail();
		//}

		//[TestMethod]
		//public void MustAllowWildcard()
		//{
		//	Assert.Fail();
		//}

		[TestMethod]
		public void MustInitializeResult()
		{
			foreach (var result in Enum.GetValues(typeof(FilterResult)))
			{
				sut.Initialize(new FilterRuleSettings { Expression = "*", Result = (FilterResult) result });
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
		public void MustValidateExpression()
		{
			var invalid = new[]
			{
				".", "+", "\"", "ç", "%", "&", "/", "(", ")", "=", "?", "^", "!", "[", "]", "{", "}", "¦", "@", "#", "°", "§", "¬", "|", "¢", "´", "'", "`", "~", "<", ">", "\\"
			};

			sut.Initialize(new FilterRuleSettings { Expression = "*" });
			sut.Initialize(new FilterRuleSettings { Expression = "a" });
			sut.Initialize(new FilterRuleSettings { Expression = "A" });
			sut.Initialize(new FilterRuleSettings { Expression = "0" });
			sut.Initialize(new FilterRuleSettings { Expression = "abcdeFGHIJK-12345" });

			foreach (var expression in invalid)
			{
				Assert.ThrowsException<ArgumentException>(() => sut.Initialize(new FilterRuleSettings { Expression = expression }));
			}
		}

		private void Execute(string expression, string[] positive, string[] negative, bool testLegacy = true)
		{
			var legacy = new LegacyFilter(expression);

			sut.Initialize(new FilterRuleSettings { Expression = expression });

			foreach (var url in positive)
			{
				Assert.IsTrue(sut.IsMatch(new Request { Url = url }), url);

				if (testLegacy)
				{
					Assert.IsTrue(legacy.IsMatch(new Uri(url)), url);
				}
			}

			foreach (var url in negative)
			{
				Assert.IsFalse(sut.IsMatch(new Request { Url = url }), url);

				if (testLegacy)
				{
					Assert.IsFalse(legacy.IsMatch(new Uri(url)), url);
				}
			}
		}
	}
}
