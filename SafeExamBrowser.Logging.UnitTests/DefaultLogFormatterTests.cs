/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Logging.UnitTests
{
	[TestClass]
	public class DefaultLogFormatterTests
	{
		[TestMethod]
		[ExpectedException(typeof(NotImplementedException))]
		public void MustReportNotYetImplementedLogContent()
		{
			var sut = new DefaultLogFormatter();

			sut.Format(new NewLogContentType());
		}

		[TestMethod]
		public void MustReturnRawTextForLogMessage()
		{
			var sut = new DefaultLogFormatter();
			var entry = new LogText("Must return this text...");

			var text = sut.Format(entry);

			Assert.AreEqual(entry.Text, text);
		}

		[TestMethod]
		public void MustCorrectlyFormatLogMessage()
		{
			var sut = new DefaultLogFormatter();
			var date = new DateTime(2017, 10, 10, 15, 24, 38);
			var threadInfo = new ThreadInfo(1234, "ABC");
			var entry = new LogMessage(date, LogLevel.Warning, "Here's a warning message...", threadInfo);

			var text = sut.Format(entry);

			Assert.AreEqual($"2017-10-10 15:24:38.000 [1234: ABC] - WARNING: Here's a warning message...", text);
		}
	}
}
