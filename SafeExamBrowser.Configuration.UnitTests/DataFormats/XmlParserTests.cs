/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.DataCompression;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.DataFormats
{
	[TestClass]
	public class XmlParserTests
	{
		private Mock<IDataCompressor> compressor;
		private Mock<ILogger> logger;
		private XmlParser sut;

		[TestInitialize]
		public void Initialize()
		{
			compressor = new Mock<IDataCompressor>();
			logger = new Mock<ILogger>();

			sut = new XmlParser(compressor.Object, logger.Object);
		}

		[TestMethod]
		public void MustOnlyParseXmlData()
		{
			Assert.IsFalse(sut.CanParse(null));
			Assert.IsFalse(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<key>someKey</key><value>1</value>"))));
			Assert.IsFalse(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<html></html>"))));
			Assert.IsFalse(sut.CanParse(new MemoryStream(Encoding.UTF8.GetBytes("<?x"))));
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
			Assert.AreEqual(false, result.RawData["anotherBoolean"]);
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

		[TestMethod]
		public void MustCorrectlyParseTestSettings()
		{
			var data = LoadTestSettings();
			var result = sut.TryParse(data);

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.AreEqual(FormatType.Xml, result.Format);
			Assert.IsNull(result.Encryption);
			Assert.IsNotNull(result.RawData);

			Assert.AreEqual(true, result.RawData[Keys.Browser.AllowConfigurationDownloads]);
			Assert.AreEqual(0, result.RawData[Keys.ConfigurationFile.ConfigurationPurpose]);
			Assert.AreEqual("https://safeexambrowser.org/start", result.RawData[Keys.Browser.StartUrl]);
			Assert.AreEqual(true, result.RawData[Keys.Keyboard.EnableF5]);
			Assert.IsInstanceOfType(result.RawData[Keys.Network.Certificates.EmbeddedCertificates], typeof(List<object>));
		}

		[TestMethod]
		public void MustCheckForRootDictionary()
		{
			var xml = "<?xml version=\"1.0\"?><plist></plist>";
			var xml2 = "<?xml version=\"1.0\"?><plist><key>someKey</key><integer>5</integer></plist>";
			var xml3 = "<?xml version=\"1.0\"?><plist><dictionary></dictionary></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.InvalidData, result.Status);

			result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml2)));
			Assert.AreEqual(LoadStatus.InvalidData, result.Status);

			result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml3)));
			Assert.AreEqual(LoadStatus.InvalidData, result.Status);
		}

		[TestMethod]
		public void MustDecompressDataIfCompressed()
		{
			var bytes = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			var data = LoadTestData();
			var stream = new MemoryStream(Encoding.UTF8.GetBytes("some incorrect data here"));

			compressor.Setup(c => c.Peek(It.IsAny<Stream>(), It.IsAny<int>())).Returns<Stream, int>((d, c) => bytes.Take(c).ToArray());
			compressor.Setup(c => c.IsCompressed(It.IsAny<Stream>())).Returns(true);

			Assert.IsTrue(sut.CanParse(stream));

			compressor.Verify(c => c.IsCompressed(It.IsAny<Stream>()), Times.Once);
			compressor.Verify(c => c.Peek(It.IsAny<Stream>(), It.IsAny<int>()), Times.Once);

			compressor.Setup(c => c.Decompress(It.IsAny<Stream>())).Returns(data);

			var result = sut.TryParse(stream);

			compressor.Verify(c => c.IsCompressed(It.IsAny<Stream>()), Times.AtLeast(2));
			compressor.Verify(c => c.Decompress(It.IsAny<Stream>()), Times.Once);

			Assert.AreEqual(LoadStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustParseEmptyXml()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.AreEqual(0, result.RawData.Count);
		}

		[TestMethod]
		public void MustDetectInvalidKey()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><nokey>blubb</nokey><true /></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.InvalidData, result.Status);
		}

		[TestMethod]
		public void MustDetectInvalidValueType()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><key>blubb</key><globb>1234</globb></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.InvalidData, result.Status);
		}

		[TestMethod]
		public void MustAllowEmptyArray()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><key>value</key><array /></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.IsInstanceOfType(result.RawData["value"], typeof(IList<object>));
		}

		[TestMethod]
		public void MustAbortParsingArrayOnError()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><key>value</key><array><blobb /></array></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.InvalidData, result.Status);
			Assert.AreEqual(0, result.RawData.Count);
		}

		[TestMethod]
		public void MustAllowEmptyDictionary()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><key>value</key><dict /></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.IsInstanceOfType(result.RawData["value"], typeof(IDictionary<string, object>));
		}

		[TestMethod]
		public void MustMapNullForEmptyStringElement()
		{
			var xml = "<?xml version=\"1.0\"?><plist><dict><key>value</key><string /></dict></plist>";
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.AreEqual(LoadStatus.Success, result.Status);
			Assert.IsNull(result.RawData["value"]);
		}

		[TestMethod]
		public void MustNotFailWithWrongData()
		{
			var result = sut.TryParse(new MemoryStream(Encoding.UTF8.GetBytes("some random text")));

			Assert.AreEqual(LoadStatus.InvalidData, result.Status);
		}

		private Stream LoadTestData()
		{
			var path = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(DataFormats)}.XmlTestData.xml";
			var data = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path);

			return data;
		}

		private Stream LoadTestSettings()
		{
			var path = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(DataFormats)}.XmlTestSettings.xml";
			var data = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path);

			return data;
		}
	}
}
