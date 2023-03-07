/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters.Rules;
using SafeExamBrowser.Settings.Browser.Filter;

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
		public void MustIgnoreCase()
		{
			sut.Initialize(new FilterRuleSettings { Expression = "http://www.test.org/path/file.txt?param=123" });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));

			sut.Initialize(new FilterRuleSettings { Expression = "HTTP://WWW.TEST.ORG/PATH/FILE.TXT?PARAM=123" });

			Assert.IsTrue(sut.IsMatch(new Request { Url = "hTtP://wWw.TeSt.OrG/pAtH/fIlE.tXt?PaRaM=123" }));
			Assert.IsTrue(sut.IsMatch(new Request { Url = "HtTp://WwW.tEst.oRg/PaTh/FiLe.TxT?pArAm=123" }));
		}

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

		[TestMethod]
		public void TestAlphanumericExpression()
		{
			var expression = "hostname123";
			var positive = new[]
			{
				$"scheme://{expression}"
			};
			var negative = new[]
			{
				$"scheme://hostname",
				$"scheme://hostname1",
				$"scheme://hostname12",
				$"scheme://hostname1234",
				$"scheme://{expression}.org",
				$"scheme://www.{expression}.org",
				$"scheme://subdomain.{expression}.com",
				$"scheme://www.realhost.{expression}",
				$"scheme://subdomain-1.subdomain-2.{expression}.org",
				$"scheme://user:password@www.{expression}.org/path/file.txt?param=123#fragment",
				$"scheme://{expression}4",
				$"scheme://hostname.org",
				$"scheme://hostname12.org",
				$"scheme://{expression}4.org",
				$"scheme://{expression}.realhost.org",
				$"scheme://subdomain.{expression}.realhost.org",
				$"{expression}://www.host.org",
				$"scheme://www.host.org/{expression}/path",
				$"scheme://www.host.org/path?param={expression}",
				$"scheme://{expression}:password@www.host.org",
				$"scheme://user:{expression}@www.host.org",
				$"scheme://user:password@www.host.org/path?param=123#{expression}"
			};

			Test(expression, positive, negative, false);
		}

		[TestMethod]
		public void TestAlphanumericExpressionWithWildcard()
		{
			var expression = "hostname*";
			var positive = new[]
			{
				"scheme://hostname.org",
				"scheme://hostnameabc.org",
				"scheme://hostname-12.org",
				"scheme://hostname-abc-def-123-456.org",
				"scheme://www.hostname-abc.org",
				"scheme://www.realhost.hostname",
				"scheme://subdomain.hostname-xyz.com",
				"scheme://hostname.realhost.org",
				"scheme://subdomain.hostname.realhost.org",
				"scheme://subdomain-1.subdomain-2.hostname-abc-123.org",
				"scheme://user:password@www.hostname-abc.org/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://hostnam.org",
				"hostname://www.host.org",
				"scheme://www.host.org/hostname/path",
				"scheme://www.host.org/path?param=hostname",
				"scheme://hostname:password@www.host.org",
				"scheme://user:hostname@www.host.org",
				"scheme://user:password@www.host.org/path?param=123#hostname"
			};

			Test(expression, positive, negative, false);
		}

		[TestMethod]
		public void TestHostWithDomain()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostWithWildcard()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostWithWildcardAsSuffix()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostWithWildcardAsPrefix()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostWithExactSubdomain()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestHostWithTrailingSlash()
		{
			var expression = "host.org/";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://host.org/",
				"scheme://host.org/url",
				"scheme://host.org/url/",
				"scheme://host.org/url/path",
				"scheme://host.org/url/path/",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.host.org/url/path/?param=123#fragment"
			};

			Test(expression, positive, Array.Empty<string>());
		}

		[TestMethod]
		public void TestHostWithoutTrailingSlash()
		{
			var expression = "host.org";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://host.org/",
				"scheme://host.org/url",
				"scheme://host.org/url/",
				"scheme://host.org/url/path",
				"scheme://host.org/url/path/",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.host.org/url/path/?param=123#fragment"
			};

			Test(expression, positive, Array.Empty<string>());
		}

		[TestMethod]
		public void TestPortNumber()
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

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestPortWildcard()
		{
			var expression = "host.org:*";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://host.org:0",
				"scheme://host.org:1",
				"scheme://host.org:2020",
				"scheme://host.org:65535",
				"scheme://www.host.org",
				"scheme://www.host.org:2",
				"scheme://www.host.org:20",
				"scheme://www.host.org:202",
				"scheme://www.host.org:2020",
				"scheme://www.host.org:20202",
				"scheme://user:password@www.host.org:2020/path/file.txt?param=123#fragment"
			};

			Test(expression, positive, Array.Empty<string>());
		}

		[TestMethod]
		public void TestPortNumberWithHostWildcard()
		{
			var expression = "*:2020";
			var positive = new[]
			{
				"scheme://host.org:2020",
				"scheme://domain.com:2020",
				"scheme://user:password@www.server.net:2020/path/file.txt?param=123#fragment"
			};
			var negative = new List<string>
			{
				"scheme://host.org"
			};

			for (var port = 0; port < 65536; port++)
			{
				if (port != 2020)
				{
					negative.Add($"{negative[0]}:{port}");
				}
			}

			Test(expression, positive, negative.ToArray());
		}

		[TestMethod]
		public void TestPath()
		{
			var expression = "host.org/url/path";
			var positive = new[]
			{
				"scheme://host.org/url/path",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org//",
				"scheme://host.org///",
				"scheme://host.org/url",
				"scheme://host.org/path",
				"scheme://host.org/url/path.txt",
				"scheme://host.org/another/url/path"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestPathWithFile()
		{
			var expression = "host.org/url/path/to/file.txt";
			var positive = new[]
			{
				"scheme://host.org/url/path/to/file.txt",
				"scheme://subdomain.host.org/url/path/to/file.txt",
				"scheme://user:password@www.host.org/url/path/to/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org/url",
				"scheme://host.org/path",
				"scheme://host.org/file.txt",
				"scheme://host.org/url/path.txt",
				"scheme://host.org/url/path/to.txt",
				"scheme://host.org/url/path/to/file",
				"scheme://host.org/url/path/to/file.",
				"scheme://host.org/url/path/to/file.t",
				"scheme://host.org/url/path/to/file.tx",
				"scheme://host.org/url/path/to/file.txt/segment",
				"scheme://host.org/url/path/to/file.txtt",
				"scheme://host.org/another/url/path/to/file.txt"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestPathWithWildcard()
		{
			var expression = "host.org/*/path";
			var positive = new[]
			{
				"scheme://host.org//path",
				"scheme://host.org/url/path",
				"scheme://host.org/another/url/path",
				"scheme://user:password@www.host.org/yet/another/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org/url",
				"scheme://host.org/path",
				"scheme://host.org/url/path.txt",
				"scheme://host.org/url/path/2"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestPathWithHostWildcard()
		{
			var expression = "*/url/path";
			var positive = new[]
			{
				"scheme://local/url/path",
				"scheme://host.org/url/path",
				"scheme://www.host.org/url/path",
				"scheme://another.server.org/url/path",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org/url",
				"scheme://host.org/path",
				"scheme://host.org/url/path.txt",
				"scheme://host.org/url/path/2"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestPathWithTrailingSlash()
		{
			var expression = "host.org/url/path/";
			var positive = new[]
			{
				"scheme://host.org/url/path",
				"scheme://host.org/url/path/",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.host.org/url/path/?param=123#fragment"
			};

			Test(expression, positive, Array.Empty<string>());
		}

		[TestMethod]
		public void TestPathWithoutTrailingSlash()
		{
			var expression = "host.org/url/path";
			var positive = new[]
			{
				"scheme://host.org/url/path",
				"scheme://host.org/url/path/",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.host.org/url/path/?param=123#fragment"
			};

			Test(expression, positive, Array.Empty<string>());
		}

		[TestMethod]
		public void TestScheme()
		{
			var expression = "scheme://host.org";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://www.host.org",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"//host.org",
				"https://host.org",
				"ftp://host.org",
				"ftps://host.org",
				"schemes://host.org"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestSchemeWithWildcard()
		{
			var expression = "*tp://host.org";
			var positive = new[]
			{
				"tp://host.org",
				"ftp://www.host.org",
				"http://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"//host.org",
				"p://host.org",
				"https://host.org",
				"ftps://host.org"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestSchemeWithHostWildcard()
		{
			var expression = "scheme://*";
			var positive = new[]
			{
				"scheme://host",
				"scheme://www.server.org",
				"scheme://subdomain.domain.org",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"//host.org",
				"http://host.org",
				"https://host.org",
				"ftp://host.org",
				"ftps://host.org",
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestUserInfoWithName()
		{
			var expression = "user@host.org";
			var positive = new[]
			{
				"scheme://user@host.org",
				"scheme://user@www.host.org",
				"scheme://user:password@host.org",
				"scheme://user:password-123@host.org",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://u@host.org",
				"scheme://us@host.org",
				"scheme://use@host.org",
				"scheme://usera@host.org",
				"scheme://user@server.net",
				"scheme://usertwo@www.host.org"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestUserInfoWithNameWildcard()
		{
			var expression = "user*@host.org";
			var positive = new[]
			{
				"scheme://user@host.org",
				"scheme://userabc@host.org",
				"scheme://user:abc@host.org",
				"scheme://user-123@www.host.org",
				"scheme://user-123:password@host.org",
				"scheme://user-123:password-123@host.org",
				"scheme://user-abc-123:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://u@host.org",
				"scheme://us@host.org",
				"scheme://use@host.org",
				"scheme://user@server.net",
				"scheme://usertwo@server.org"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestUserInfoWithPassword()
		{
			var expression = "user:password@host.org";
			var positive = new[]
			{
				"scheme://user:password@host.org",
				"scheme://user:password@www.host.org",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://u@host.org",
				"scheme://us@host.org",
				"scheme://use@host.org",
				"scheme://user@server.net",
				"scheme://usertwo@server.org",
				"scheme://user@host.org",
				"scheme://userabc@host.org",
				"scheme://user:abc@host.org",
				"scheme://user-123@www.host.org",
				"scheme://user-123:password@host.org",
				"scheme://user-123:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password-123@host.org",
				"scheme://user:password-123@www.host.org/url/path?param=123#fragment"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestUserInfoWithWildcard()
		{
			var expression = "*@host.org";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://user@host.org",
				"scheme://user:password@host.org",
				"scheme://www.host.org/url/path?param=123#fragment",
				"scheme://user@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://server.org",
				"scheme://user@server.org",
				"scheme://www.server.org/url/path?param=123#fragment",
				"scheme://user:password@www.server.org/url/path?param=123#fragment"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestUserInfoWithHostWildcard()
		{
			var expression = "user:password@*";
			var positive = new[]
			{
				"scheme://user:password@host.org",
				"scheme://user:password@server.net",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://server.org",
				"scheme://user@host.org",
				"scheme://user@server.org",
				"scheme://password@host.org",
				"scheme://www.host.org/url/path?param=123#fragment",
				"scheme://www.server.org/url/path?param=123#fragment",
				"scheme://user@www.host.org/url/path?param=123#fragment",
				"scheme://user@www.server.org/url/path?param=123#fragment",
				"scheme://password@www.server.org/url/path?param=123#fragment"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestQuery()
		{
			var expression = "host.org?param=123";
			var positive = new[]
			{
				"scheme://host.org?param=123",
				"scheme://www.host.org/?param=123",
				"scheme://www.host.org/path/?param=123",
				"scheme://www.host.org/some/other/random/path?param=123",
				"scheme://user:password@www.host.org/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org?",
				"scheme://host.org?=",
				"scheme://host.org?=123",
				"scheme://host.org?param=",
				"scheme://host.org?param=1",
				"scheme://host.org?param=12",
				"scheme://host.org?param=1234"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestQueryWithWildcardAsPrefix()
		{
			var expression = "host.org?*param=123";
			var positive = new[]
			{
				"scheme://host.org?param=123",
				"scheme://www.host.org?param=123",
				"scheme://www.host.org/path/?param=123",
				"scheme://www.host.org/some/other/random/path?param=123",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://host.org?other_param=456&param=123",
				"scheme://host.org?param=123&another_param=123",
				"scheme://www.host.org?other_param=456&param=123",
				"scheme://www.host.org/path/?other_param=456&param=123",
				"scheme://www.host.org/some/other/random/path?other_param=456&param=123",
				"scheme://user:password@www.host.org/url/path?other_param=456&param=123#fragment",
				"scheme://host.org?some_param=123469yvuiopwo&another_param=some%20whitespaces%26special%20characters%2B%22%2A%25%C3%A7%2F%28&param=123"
			};
			var negative = new[]
			{
				"scheme://host.org?",
				"scheme://host.org?=",
				"scheme://host.org?=123",
				"scheme://host.org?aram=123",
				"scheme://host.org?param=",
				"scheme://host.org?param=1",
				"scheme://host.org?param=12",
				"scheme://host.org?param=1234",
				"scheme://host.org?param=123&another_param=456"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestQueryWithWildcardAsSuffix()
		{
			var expression = "host.org?param=123*";
			var positive = new[]
			{
				"scheme://host.org?param=123",
				"scheme://host.org?param=1234",
				"scheme://www.host.org?param=123",
				"scheme://www.host.org/path/?param=123",
				"scheme://host.org?param=123&another_param=456",
				"scheme://www.host.org/some/other/random/path?param=123",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://host.org?param=123&other_param=456",
				"scheme://www.host.org/path/?param=123&other_param=456",
				"scheme://www.host.org/some/other/random/path?param=123&other_param=456",
				"scheme://user:password@www.host.org/url/path?param=123&other_param=456#fragment",
				"scheme://host.org?param=123&some_param=123469yvuiopwo&another_param=some%20whitespaces%26special%20characters%2B%22%2A%25%C3%A7%2F%28"
			};
			var negative = new[]
			{
				"scheme://host.org?",
				"scheme://host.org?=",
				"scheme://host.org?=123",
				"scheme://host.org?aram=123",
				"scheme://host.org?param=",
				"scheme://host.org?param=1",
				"scheme://host.org?param=12",
				"scheme://host.org?aparam=123",
				"scheme://www.host.org?param=456&param=123",
				"scheme://host.org?some_param=123469yvuiopwo&another_param=some%20whitespaces%26special%20characters%2B%22%2A%25%C3%A7%2F%28&param=123"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestQueryNotAllowed()
		{
			var expression = "host.org?.";
			var positive = new[]
			{
				"scheme://host.org",
				"scheme://host.org?",
				"scheme://user:password@www.host.org/url/path#fragment",
				"scheme://user:password@www.host.org/url/path?#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org?a",
				"scheme://host.org?%20",
				"scheme://host.org?=",
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestQueryWithHostWildcard()
		{
			var expression = "*?param=123";
			var positive = new[]
			{
				"scheme://host.org?param=123",
				"scheme://server.net?param=123",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.server.net/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org?param=1234",
				"scheme://host.org?param=12",
				"scheme://host.org?",
				"scheme://host.org?param",
				"scheme://host.org?123",
				"scheme://host.org?="
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestFragment()
		{
			var expression = "host.org#fragment";
			var positive = new[]
			{
				"scheme://host.org#fragment",
				"scheme://www.host.org#fragment",
				"scheme://user:password@www.host.org/url/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org#",
				"scheme://host.org#fragmen",
				"scheme://host.org#fragment123",
				"scheme://host.org#otherfragment"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestFragmentWithWildcardAsPrefix()
		{
			var expression = "host.org#*fragment";
			var positive = new[]
			{
				"scheme://host.org#fragment",
				"scheme://host.org#somefragment",
				"scheme://www.host.org#another_fragment",
				"scheme://user:password@www.host.org/url/path/file.txt?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org#",
				"scheme://host.org#fragmen",
				"scheme://host.org#fragment123"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestFragmentWithWildcardAsSuffix()
		{
			var expression = "host.org#fragment*";
			var positive = new[]
			{
				"scheme://host.org#fragment",
				"scheme://host.org#fragment-123",
				"scheme://user:password@www.host.org/url/path/file.txt?param=123#fragment_abc"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org#",
				"scheme://host.org#fragmen",
				"scheme://www.host.org#another_fragment"
			};

			Test(expression, positive, negative);
		}

		[TestMethod]
		public void TestFragmentWithHostWildcard()
		{
			var expression = "*#fragment";
			var positive = new[]
			{
				"scheme://host.org#fragment",
				"scheme://server.net#fragment",
				"scheme://user:password@www.host.org/url/path?param=123#fragment",
				"scheme://user:password@www.server.net/url/path?param=123#fragment"
			};
			var negative = new[]
			{
				"scheme://host.org",
				"scheme://host.org#",
				"scheme://host.org#fragmen",
				"scheme://host.org#fragment123"
			};

			Test(expression, positive, negative);
		}

		private void Test(string expression, string[] positive, string[] negative, bool testLegacy = true)
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
