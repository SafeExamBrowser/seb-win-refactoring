/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Core.I18n;

namespace SafeExamBrowser.Core.UnitTests.I18n
{
	[TestClass]
	public class XmlTextResourceTests
	{
		[TestMethod]
		public void MustCorrectlyLoadData()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = Path.GetDirectoryName(location) + $@"\{nameof(I18n)}\Text_Valid.xml";
			var sut = new XmlTextResource(path);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.IsTrue(text.Count == 2);
			Assert.AreEqual("Application Log", text[TextKey.LogWindow_Title]);
			Assert.AreEqual("Version", text[TextKey.Version]);
		}

		[TestMethod]
		[ExpectedException(typeof(XmlException))]
		public void MustFailWithInvalidData()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = Path.GetDirectoryName(location) + $@"\{nameof(I18n)}\Text_Invalid.txt";
			var sut = new XmlTextResource(path);

			sut.LoadText();
		}

		[TestMethod]
		public void MustNeverReturnNull()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResourceTests)).Location;
			var path = Path.GetDirectoryName(location) + $@"\{nameof(I18n)}\Text_Incompatible.xml";
			var sut = new XmlTextResource(path);

			var text = sut.LoadText();

			Assert.IsNotNull(text);
			Assert.IsTrue(text.Count == 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustNotAcceptInvalidPath()
		{
			new XmlTextResource("This is not a valid path");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAcceptNullAsPath()
		{
			new XmlTextResource(null);
		}
	}
}
