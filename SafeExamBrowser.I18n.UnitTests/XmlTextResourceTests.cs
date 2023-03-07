/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.I18n.UnitTests
{
	[TestClass]
	public class XmlTextResourceTests
	{
		[TestMethod]
		public void MustCorrectlyLoadData()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text_Valid.xml";
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var sut = new XmlTextResource(stream);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.IsTrue(text.Count == 4);
			Assert.AreEqual("Application Log", text[TextKey.LogWindow_Title]);
			Assert.AreEqual("Version", text[TextKey.Version]);
		}

		[TestMethod]
		[ExpectedException(typeof(XmlException))]
		public void MustFailWithInvalidData()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text_Invalid.txt";
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var sut = new XmlTextResource(stream);

			sut.LoadText();
		}

		[TestMethod]
		public void MustNeverReturnNull()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text_Incompatible.xml";
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var sut = new XmlTextResource(stream);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.IsTrue(text.Count == 0);
		}

		[TestMethod]
		public void MustNeverSetNullValue()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text_Valid.xml";
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var sut = new XmlTextResource(stream);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.AreEqual(string.Empty, text[TextKey.Notification_LogTooltip]);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAcceptNullAsPath()
		{
			new XmlTextResource(null);
		}

		[TestMethod]
		public void MustTrimValues()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text_Valid.xml";
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var sut = new XmlTextResource(stream);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.AreEqual("Hello world", text[TextKey.Notification_AboutTooltip]);
		}
	}
}
