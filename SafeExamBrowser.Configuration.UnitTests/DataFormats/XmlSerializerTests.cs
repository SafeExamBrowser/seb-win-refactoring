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
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.DataFormats
{
	[TestClass]
	public class XmlSerializerTests
	{
		private Mock<ILogger> logger;
		private XmlSerializer sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new XmlSerializer(logger.Object);
		}

		[TestMethod]
		public void MustOnlySupportXmlFormat()
		{
			var values = Enum.GetValues(typeof(FormatType));

			foreach (var value in values)
			{
				if (value is FormatType format && format != FormatType.Xml)
				{
					Assert.IsFalse(sut.CanSerialize(format));
				}
			}

			Assert.IsTrue(sut.CanSerialize(FormatType.Xml));
		}

		[TestMethod]
		public void MustCorrectlySerializeEmptyDictionary()
		{
			var result = sut.TrySerialize(new Dictionary<string, object>());

			Assert.AreEqual(SaveStatus.Success, result.Status);
			Assert.IsNotNull(result.Data);
			Assert.AreNotEqual(0, result.Data.Length);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());
			Assert.AreEqual(XmlElement.Dictionary, (xml.Root.Nodes().First() as XElement).Name);
			Assert.IsTrue((xml.Root.Nodes().First() as XElement).IsEmpty);
		}

		[TestMethod]
		public void MustCorrectlySerializeDictionary()
		{
			var data = new Dictionary<string, object> { { "abc", 1 }, { "def", 2 }, { "ghi", 3 } };
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.Success, result.Status);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());

			var dictionary = xml.Root.Nodes().First() as XElement;

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(0) as XElement).Name);
			Assert.AreEqual("abc", (dictionary.Nodes().ElementAt(0) as XElement).Value);
			Assert.AreEqual(XmlElement.Integer, (dictionary.Nodes().ElementAt(1) as XElement).Name);
			Assert.AreEqual(1, Convert.ToInt32((dictionary.Nodes().ElementAt(1) as XElement).Value));
			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(2) as XElement).Name);
			Assert.AreEqual("def", (dictionary.Nodes().ElementAt(2) as XElement).Value);
			Assert.AreEqual(XmlElement.Integer, (dictionary.Nodes().ElementAt(3) as XElement).Name);
			Assert.AreEqual(2, Convert.ToInt32((dictionary.Nodes().ElementAt(3) as XElement).Value));
			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(4) as XElement).Name);
			Assert.AreEqual("ghi", (dictionary.Nodes().ElementAt(4) as XElement).Value);
			Assert.AreEqual(XmlElement.Integer, (dictionary.Nodes().ElementAt(5) as XElement).Name);
			Assert.AreEqual(3, Convert.ToInt32((dictionary.Nodes().ElementAt(5) as XElement).Value));
		}

		[TestMethod]
		public void MustCorrectlySerializeArray()
		{
			var data = new Dictionary<string, object> { { "test", new List<object> { 12.3, 23.4, 34.5, 45.6 } } };
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.Success, result.Status);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());

			var dictionary = xml.Root.Nodes().First() as XElement;
			var array = dictionary.Nodes().ElementAt(1) as XElement;
			var values = array.Nodes().Cast<XElement>();

			Assert.AreEqual(XmlElement.Array, array.Name);
			Assert.AreEqual(4, values.Count());
			Assert.AreEqual(XmlElement.Real, values.ElementAt(0).Name);
			Assert.AreEqual(12.3, Convert.ToDouble(values.ElementAt(0).Value));
			Assert.AreEqual(XmlElement.Real, values.ElementAt(1).Name);
			Assert.AreEqual(23.4, Convert.ToDouble(values.ElementAt(1).Value));
			Assert.AreEqual(XmlElement.Real, values.ElementAt(2).Name);
			Assert.AreEqual(34.5, Convert.ToDouble(values.ElementAt(2).Value));
			Assert.AreEqual(XmlElement.Real, values.ElementAt(3).Name);
			Assert.AreEqual(45.6, Convert.ToDouble(values.ElementAt(3).Value));
		}

		[TestMethod]
		public void MustCorrectlySerializeNestedCollections()
		{
			var data = new Dictionary<string, object>
			{
				{ "ArrayOfDictionaries",
					new List<object>
					{
						new Dictionary<string, object>
						{
							{ "Double", 123.5 },
							{ "Text", "Hello World!" }
						},
						new Dictionary<string, object>
						{
							{ "InnerArray", new List<object> { 6, 7, 8 } }
						}
					}
				},
				{ "SomeDictionary", new Dictionary<string, object> { { "blubb", 123 } } },
				{ "SomeArray", new List<object> { 1, 2, 3, 4 } }
			};
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.Success, result.Status);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());

			var dictionary = xml.Root.Nodes().First() as XElement;
			var arrayOfDictionaries = dictionary.Nodes().ElementAt(1) as XElement;
			var innerDictionaryOne = arrayOfDictionaries.Nodes().ElementAt(0) as XElement;
			var innerDictionaryTwo = arrayOfDictionaries.Nodes().ElementAt(1) as XElement;
			var innerDictionaryTwoArray = innerDictionaryTwo.Nodes().ElementAt(1) as XElement;
			var someArray = dictionary.Nodes().ElementAt(3) as XElement;
			var someDictionary = dictionary.Nodes().ElementAt(5) as XElement;

			Assert.AreEqual(6, dictionary.Nodes().Count());
			Assert.AreEqual(2, arrayOfDictionaries.Nodes().Count());
			Assert.AreEqual(4, someArray.Nodes().Count());
			Assert.AreEqual(2, someDictionary.Nodes().Count());

			Assert.AreEqual(XmlElement.Array, arrayOfDictionaries.Name);
			Assert.AreEqual(XmlElement.Array, someArray.Name);
			Assert.AreEqual(XmlElement.Dictionary, someDictionary.Name);

			Assert.AreEqual(123.5, Convert.ToDouble((innerDictionaryOne.Nodes().ElementAt(1) as XElement).Value));
			Assert.AreEqual("Hello World!", (innerDictionaryOne.Nodes().ElementAt(3) as XElement).Value);
			Assert.AreEqual(6, Convert.ToInt32((innerDictionaryTwoArray.Nodes().ElementAt(0) as XElement).Value));
			Assert.AreEqual(7, Convert.ToInt32((innerDictionaryTwoArray.Nodes().ElementAt(1) as XElement).Value));
			Assert.AreEqual(8, Convert.ToInt32((innerDictionaryTwoArray.Nodes().ElementAt(2) as XElement).Value));

			Assert.AreEqual(1, Convert.ToInt32((someArray.Nodes().ElementAt(0) as XElement).Value));
			Assert.AreEqual(2, Convert.ToInt32((someArray.Nodes().ElementAt(1) as XElement).Value));
			Assert.AreEqual(3, Convert.ToInt32((someArray.Nodes().ElementAt(2) as XElement).Value));
			Assert.AreEqual(4, Convert.ToInt32((someArray.Nodes().ElementAt(3) as XElement).Value));

			Assert.AreEqual(123, Convert.ToInt32((someDictionary.Nodes().ElementAt(1) as XElement).Value));
		}

		[TestMethod]
		public void MustCorectlySerializeSimpleTypes()
		{
			var data = new Dictionary<string, object>
			{
				{ "SomeData", Convert.FromBase64String("/5vlF76sb+5vgkhjiNTOn7l1SN3Ho2UAMJD3TtLo49M=") },
				{ "SomeDate", new DateTime(2019, 02, 20, 14, 30, 00, 123, DateTimeKind.Utc) },
				{ "SomeBoolean", true },
				{ "AnotherBoolean", false },
				{ "SomeInteger", 4567 },
				{ "SomeDouble", 4567.8912 },
				{ "SomeText", "Here goes some text" }
			};
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.Success, result.Status);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());

			var dictionary = xml.Root.Nodes().First() as XElement;

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(0) as XElement).Name);
			Assert.AreEqual("AnotherBoolean", (dictionary.Nodes().ElementAt(0) as XElement).Value);
			Assert.AreEqual(false, Convert.ToBoolean((dictionary.Nodes().ElementAt(1) as XElement).Name.LocalName));

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(2) as XElement).Name);
			Assert.AreEqual("SomeBoolean", (dictionary.Nodes().ElementAt(2) as XElement).Value);
			Assert.AreEqual(true, Convert.ToBoolean((dictionary.Nodes().ElementAt(3) as XElement).Name.LocalName));

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(4) as XElement).Name);
			Assert.AreEqual("SomeData", (dictionary.Nodes().ElementAt(4) as XElement).Value);
			Assert.AreEqual(XmlElement.Data, (dictionary.Nodes().ElementAt(5) as XElement).Name);
			Assert.AreEqual("/5vlF76sb+5vgkhjiNTOn7l1SN3Ho2UAMJD3TtLo49M=", (dictionary.Nodes().ElementAt(5) as XElement).Value);

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(6) as XElement).Name);
			Assert.AreEqual("SomeDate", (dictionary.Nodes().ElementAt(6) as XElement).Value);
			Assert.AreEqual(XmlElement.Date, (dictionary.Nodes().ElementAt(7) as XElement).Name);
			Assert.AreEqual("2019-02-20T14:30:00.123Z", (dictionary.Nodes().ElementAt(7) as XElement).Value);

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(8) as XElement).Name);
			Assert.AreEqual("SomeDouble", (dictionary.Nodes().ElementAt(8) as XElement).Value);
			Assert.AreEqual(XmlElement.Real, (dictionary.Nodes().ElementAt(9) as XElement).Name);
			Assert.AreEqual(4567.8912, Convert.ToDouble((dictionary.Nodes().ElementAt(9) as XElement).Value));

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(10) as XElement).Name);
			Assert.AreEqual("SomeInteger", (dictionary.Nodes().ElementAt(10) as XElement).Value);
			Assert.AreEqual(XmlElement.Integer, (dictionary.Nodes().ElementAt(11) as XElement).Name);
			Assert.AreEqual(4567, Convert.ToInt32((dictionary.Nodes().ElementAt(11) as XElement).Value));

			Assert.AreEqual(XmlElement.Key, (dictionary.Nodes().ElementAt(12) as XElement).Name);
			Assert.AreEqual("SomeText", (dictionary.Nodes().ElementAt(12) as XElement).Value);
			Assert.AreEqual(XmlElement.String, (dictionary.Nodes().ElementAt(13) as XElement).Name);
			Assert.AreEqual("Here goes some text", (dictionary.Nodes().ElementAt(13) as XElement).Value);
		}

		[TestMethod]
		public void MustSerializeNullAsString()
		{
			var data = new Dictionary<string, object> { { "test", null } };
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.Success, result.Status);

			result.Data.Seek(0, SeekOrigin.Begin);

			var xml = XDocument.Load(result.Data);

			Assert.AreEqual(XmlElement.Root, xml.Root.Name);
			Assert.AreEqual(1, xml.Root.Nodes().Count());

			var dictionary = xml.Root.Nodes().First() as XElement;

			Assert.IsTrue((dictionary.Nodes().ElementAt(1) as XElement).IsEmpty);
			Assert.AreEqual(XmlElement.String, (dictionary.Nodes().ElementAt(1) as XElement).Name);
		}

		[TestMethod]
		public void MustFailForUnknownSimpleType()
		{
			var data = new Dictionary<string, object> { { "test", new Tuple<int>(123) } };
			var result = sut.TrySerialize(data);

			Assert.AreEqual(SaveStatus.InvalidData, result.Status);
		}
	}
}
