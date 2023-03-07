/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.I18n.UnitTests
{
	[TestClass]
	public class TextTests
	{
		private Mock<ILogger> loggerMock;
		private Text sut;

		[TestInitialize]
		public void Initialize()
		{
			loggerMock = new Mock<ILogger>();
			sut = new Text(loggerMock.Object);
		}

		[TestMethod]
		public void MustNeverReturnNull()
		{
			var text = sut.Get((TextKey)(-1));

			Assert.IsNotNull(text);
		}

		[TestMethod]
		public void MustNotFailToInitializeWhenDataNotFound()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			sut.Initialize();
		}
	}
}
