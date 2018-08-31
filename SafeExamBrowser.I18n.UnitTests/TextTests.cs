/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.I18n;

namespace SafeExamBrowser.I18n.UnitTests
{
	[TestClass]
	public class TextTests
	{
		private Mock<ILogger> loggerMock;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
		}

		[TestMethod]
		public void MustNeverReturnNull()
		{
			var sut = new Text(loggerMock.Object);
			var text = sut.Get((TextKey)(-1));

			Assert.IsNotNull(text);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullResource()
		{
			var sut = new Text(loggerMock.Object);

			sut.Initialize(null);
		}

		[TestMethod]
		public void MustNotFailWhenGettingNullFromResource()
		{
			var resource = new Mock<ITextResource>();
			var sut = new Text(loggerMock.Object);

			resource.Setup(r => r.LoadText()).Returns<IDictionary<TextKey, string>>(null);
			sut.Initialize(resource.Object);

			var text = sut.Get((TextKey)(-1));

			Assert.IsNotNull(text);
		}

		[TestMethod]
		public void MustNotFailWhenResourceThrowsException()
		{
			var resource = new Mock<ITextResource>();
			var sut = new Text(loggerMock.Object);

			resource.Setup(r => r.LoadText()).Throws<Exception>();
			sut.Initialize(resource.Object);

			var text = sut.Get((TextKey)(-1));

			loggerMock.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);

			Assert.IsNotNull(text);
		}
	}
}
