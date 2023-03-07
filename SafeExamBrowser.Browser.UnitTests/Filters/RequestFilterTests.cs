/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Settings.Browser.Filter;

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
			var allow = new Mock<IRule>();
			var block = new Mock<IRule>();

			allow.SetupGet(r => r.Result).Returns(FilterResult.Allow);
			block.SetupGet(r => r.Result).Returns(FilterResult.Block);
			block.Setup(r => r.IsMatch(It.IsAny<Request>())).Returns(true);

			sut.Load(allow.Object);
			sut.Load(block.Object);

			var result = sut.Process(new Request());

			allow.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Never);
			block.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Once);

			Assert.AreEqual(FilterResult.Block, result);
		}

		[TestMethod]
		public void MustProcessAllowRulesSecond()
		{
			var allow = new Mock<IRule>();
			var block = new Mock<IRule>();

			allow.SetupGet(r => r.Result).Returns(FilterResult.Allow);
			allow.Setup(r => r.IsMatch(It.IsAny<Request>())).Returns(true);
			block.SetupGet(r => r.Result).Returns(FilterResult.Block);

			sut.Load(allow.Object);
			sut.Load(block.Object);

			var result = sut.Process(new Request());

			allow.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Once);
			block.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Once);

			Assert.AreEqual(FilterResult.Allow, result);
		}

		[TestMethod]
		public void MustReturnDefaultWithoutMatch()
		{
			var allow = new Mock<IRule>();
			var block = new Mock<IRule>();

			allow.SetupGet(r => r.Result).Returns(FilterResult.Allow);
			block.SetupGet(r => r.Result).Returns(FilterResult.Block);

			sut.Default = (FilterResult) (-1);
			sut.Load(allow.Object);
			sut.Load(block.Object);

			var result = sut.Process(new Request());

			allow.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Once);
			block.Verify(r => r.IsMatch(It.IsAny<Request>()), Times.Once);

			Assert.AreEqual((FilterResult) (-1), result);
		}

		[TestMethod]
		public void MustReturnDefaultWithoutRules()
		{
			sut.Default = FilterResult.Allow;
			var result = sut.Process(new Request());
			Assert.AreEqual(FilterResult.Allow, result);

			sut.Default = FilterResult.Block;
			result = sut.Process(new Request());
			Assert.AreEqual(FilterResult.Block, result);
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void MustNotAllowUnsupportedResult()
		{
			var rule = new Mock<IRule>();

			rule.SetupGet(r => r.Result).Returns((FilterResult) (-1));
			sut.Load(rule.Object);
		}
	}
}
