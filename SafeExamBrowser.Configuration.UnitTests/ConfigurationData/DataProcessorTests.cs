/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Configuration.UnitTests.ConfigurationData
{
	[TestClass]
	public class DataProcessorTests
	{
		private DataProcessor sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new DataProcessor();
		}

		[TestMethod]
		public void MustAllowBrowserToolbarForReloading()
		{
			var raw = new Dictionary<string, object>();
			var settings = new AppSettings();

			raw.Add(Keys.Browser.ShowReloadButton, true);
			settings.Browser.AdditionalWindow.AllowReloading = false;
			settings.Browser.AdditionalWindow.ShowReloadButton = false;
			settings.Browser.MainWindow.AllowReloading = false;
			settings.Browser.MainWindow.ShowReloadButton = false;

			sut.Process(raw, settings);

			Assert.IsFalse(settings.Browser.AdditionalWindow.ShowToolbar);
			Assert.IsFalse(settings.Browser.MainWindow.ShowToolbar);

			settings.Browser.AdditionalWindow.AllowReloading = true;
			settings.Browser.AdditionalWindow.ShowReloadButton = true;
			settings.Browser.MainWindow.AllowReloading = true;
			settings.Browser.MainWindow.ShowReloadButton = true;

			sut.Process(raw, settings);

			Assert.IsTrue(settings.Browser.AdditionalWindow.ShowToolbar);
			Assert.IsTrue(settings.Browser.MainWindow.ShowToolbar);
		}

		[TestMethod]
		public void MustCalculateCorrectConfigurationKey()
		{
			var formatter = new BinaryFormatter();
			var path1 = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(ConfigurationData)}.TestDictionary1.bin";
			var path2 = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(ConfigurationData)}.TestDictionary2.bin";
			var path3 = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.{nameof(ConfigurationData)}.TestDictionary3.bin";
			var stream1 = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path1);
			var stream2 = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path2);
			var stream3 = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path3);
			var data1 = formatter.Deserialize(stream1) as IDictionary<string, object>;
			var data2 = formatter.Deserialize(stream2) as IDictionary<string, object>;
			var data3 = formatter.Deserialize(stream3) as IDictionary<string, object>;
			var settings1 = new AppSettings();
			var settings2 = new AppSettings();
			var settings3 = new AppSettings();

			sut.Process(data1, settings1);
			sut.Process(data2, settings2);
			sut.Process(data3, settings3);

			Assert.AreEqual("6063c3351ed1ac878c05072598d5079e30ca763c957d8e04bd45131c08f88d1a", settings1.Browser.ConfigurationKey);
			Assert.AreEqual("4fc002d2ae4faf994a14bede54d95ac58a1a2cb9b59bc5b4277ff29559b46e3d", settings2.Browser.ConfigurationKey);
			Assert.AreEqual("ab426e25b795c917f1fb40f7ef8e5757ef97d7c7ad6792e655c4421d47329d7a", settings3.Browser.ConfigurationKey);
		}

		[TestMethod]
		public void MustRemoveLegacyBrowserEngines()
		{
			var settings = new AppSettings();
			var firefox = new WhitelistApplication { ExecutablePath = @"C:\Program Files (x86)\Mozilla Firefox", ExecutableName = "firefox.exe" };

			settings.Applications.Whitelist.Add(new WhitelistApplication { ExecutablePath = @"C:\some\path\xulrunner\etc", ExecutableName = "firefox.exe" });
			settings.Applications.Whitelist.Add(new WhitelistApplication { ExecutablePath = @"C:\some\path\xulrunner\etc", ExecutableName = "xulrunner.exe" });
			settings.Applications.Whitelist.Add(firefox);

			sut.Process(Mock.Of<IDictionary<string, object>>(), settings);

			Assert.AreEqual(1, settings.Applications.Whitelist.Count);
			Assert.AreSame(firefox, settings.Applications.Whitelist[0]);
		}
	}
}
