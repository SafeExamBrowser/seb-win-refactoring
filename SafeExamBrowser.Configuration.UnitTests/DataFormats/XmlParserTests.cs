/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.UnitTests.DataFormats
{
	[TestClass]
	public class XmlParserTests
	{
		private Mock<ILogger> logger;
		private XmlParser sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();

			sut = new XmlParser(logger.Object);
		}

		[TestMethod]
		public void MustOnlyParseXmlData()
		{
			Assert.IsFalse(sut.CanParse(null));
			Assert.IsFalse(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<key>someKey</key><value>1</value>"))));
			Assert.IsFalse(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<html></html>"))));
			Assert.IsTrue(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"))));
		}

		[TestMethod]
		public void MustCorrectlyParseXml()
		{
			var data = LoadTestData();
			var result = sut.TryParse(data);

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.AreEqual(FormatType.Xml, result.Format);
			Assert.IsNull(result.Encryption);
			Assert.IsNotNull(result.RawData);

			Assert.AreEqual("test123", result.RawData["someString"]);
			Assert.AreEqual(9876, result.RawData["someInteger"]);
			Assert.IsTrue(Convert.FromBase64String("/5vlF76sb+5vgkhjiNTOn7l1SN3Ho2UAMJD3TtLo49M=").SequenceEqual(result.RawData["someData"] as IEnumerable<byte>));
			Assert.AreEqual(true, result.RawData["someBoolean"]);
			Assert.AreEqual(12.34, result.RawData["someReal"]);
			Assert.AreEqual(new DateTime(2019, 02, 20, 12, 30, 00, 123), result.RawData["someDate"]);

			var array = result.RawData["anArray"] as List<object>;
			var dictOne = array[0] as Dictionary<string, object>;
			var dictTwo = array[1] as Dictionary<string, object>;
			var dictThree = array[2] as Dictionary<string, object>;

			Assert.AreEqual(3, dictOne["dictOneKeyOne"]);
			Assert.AreEqual(4, dictOne["dictOneKeyTwo"]);
			Assert.AreEqual(5, dictTwo["dictTwoKeyOne"]);
			Assert.AreEqual(6, dictTwo["dictTwoKeyTwo"]);
			Assert.AreEqual(7, dictThree["dictThreeKeyOne"]);
			Assert.AreEqual(1, (dictThree["dictThreeKeyTwo"] as List<object>)[0]);
			Assert.AreEqual(2, (dictThree["dictThreeKeyTwo"] as List<object>)[1]);
			Assert.AreEqual(3, (dictThree["dictThreeKeyTwo"] as List<object>)[2]);
			Assert.AreEqual(4, (dictThree["dictThreeKeyTwo"] as List<object>)[3]);
		}

		private Stream LoadTestData()
		{
			var path = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(DataFormats)}.XmlTestData.xml";
			var data = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path);

			return data;
		}
	}
}
