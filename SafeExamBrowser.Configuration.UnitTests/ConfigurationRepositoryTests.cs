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
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Configuration.Contracts.DataResources;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests
{
	[TestClass]
	public class ConfigurationRepositoryTests
	{
		private ConfigurationRepository sut;
		private Mock<IDataParser> binaryParser;
		private Mock<IDataSerializer> binarySerializer;
		private Mock<ICertificateStore> certificateStore;
		private Mock<IResourceLoader> fileLoader;
		private Mock<IResourceSaver> fileSaver;
		private Mock<IModuleLogger> logger;
		private Mock<IResourceLoader> networkLoader;
		private Mock<IDataParser> xmlParser;
		private Mock<IDataSerializer> xmlSerializer;

		[TestInitialize]
		public void Initialize()
		{
			binaryParser = new Mock<IDataParser>();
			binarySerializer = new Mock<IDataSerializer>();
			certificateStore = new Mock<ICertificateStore>();
			fileLoader = new Mock<IResourceLoader>();
			fileSaver = new Mock<IResourceSaver>();
			logger = new Mock<IModuleLogger>();
			networkLoader = new Mock<IResourceLoader>();
			xmlParser = new Mock<IDataParser>();
			xmlSerializer = new Mock<IDataSerializer>();

			fileLoader.Setup(f => f.CanLoad(It.IsAny<Uri>())).Returns<Uri>(u => u.IsFile);
			fileSaver.Setup(f => f.CanSave(It.IsAny<Uri>())).Returns<Uri>(u => u.IsFile);
			networkLoader.Setup(n => n.CanLoad(It.IsAny<Uri>())).Returns<Uri>(u => u.Scheme.Equals("http") || u.Scheme.Equals("seb"));

			SetEntryAssembly();

			sut = new ConfigurationRepository(certificateStore.Object, logger.Object);
			sut.InitializeAppConfig();
		}

		[TestMethod]
		public void ConfigureClient_MustWorkAsExpected()
		{
			var stream = new MemoryStream() as Stream;
			var password = new PasswordParameters { Password = "test123" };
			var parseResult = new ParseResult
			{
				Format = FormatType.Binary,
				RawData = new Dictionary<string, object>(),
				Status = LoadStatus.Success
			};
			var serializeResult = new SerializeResult
			{
				Data = new MemoryStream(),
				Status = SaveStatus.Success
			};

			RegisterModules();

			fileLoader.Setup(n => n.TryLoad(It.IsAny<Uri>(), out stream)).Returns(LoadStatus.Success);
			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Returns(true);
			binaryParser.Setup(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>())).Returns(parseResult);
			binarySerializer.Setup(b => b.CanSerialize(FormatType.Binary)).Returns(true);
			binarySerializer.Setup(b => b.TrySerialize(It.IsAny<Dictionary<string, object>>(), It.IsAny<PasswordParameters>())).Returns(serializeResult);
			fileSaver.Setup(f => f.TrySave(It.IsAny<Uri>(), It.IsAny<Stream>())).Returns(SaveStatus.Success);

			var status = sut.ConfigureClientWith(new Uri("C:\\TEMP\\Some\\file.seb"), password);

			fileLoader.Verify(n => n.TryLoad(It.IsAny<Uri>(), out stream), Times.Once);
			binaryParser.Verify(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>()), Times.Once);
			certificateStore.Verify(c => c.ExtractAndImportIdentities(It.IsAny<Dictionary<string, object>>()), Times.Once);
			binarySerializer.Verify(b => b.TrySerialize(
				It.IsAny<Dictionary<string, object>>(),
				It.Is<PasswordParameters>(p => p.IsHash == true && p.Password == string.Empty)), Times.Once);
			fileSaver.Verify(f => f.TrySave(It.IsAny<Uri>(), It.IsAny<Stream>()), Times.Once);

			Assert.AreEqual(SaveStatus.Success, status);
		}

		[TestMethod]
		public void ConfigureClient_MustKeepSameEncryptionAccordingToConfiguration()
		{
			var stream = new MemoryStream() as Stream;
			var password = new PasswordParameters { Password = "test123" };
			var parseResult = new ParseResult
			{
				Encryption = new PublicKeyParameters
				{
					InnerEncryption = password,
					SymmetricEncryption = true
				},
				Format = FormatType.Binary,
				RawData = new Dictionary<string, object> { { Keys.ConfigurationFile.KeepClientConfigEncryption, true } },
				Status = LoadStatus.Success
			};
			var serializeResult = new SerializeResult
			{
				Data = new MemoryStream(),
				Status = SaveStatus.Success
			};

			RegisterModules();

			fileLoader.Setup(n => n.TryLoad(It.IsAny<Uri>(), out stream)).Returns(LoadStatus.Success);
			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Returns(true);
			binaryParser.Setup(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>())).Returns(parseResult);
			binarySerializer.Setup(b => b.CanSerialize(FormatType.Binary)).Returns(true);
			binarySerializer.Setup(b => b.TrySerialize(It.IsAny<Dictionary<string, object>>(), It.IsAny<EncryptionParameters>())).Returns(serializeResult);
			fileSaver.Setup(f => f.TrySave(It.IsAny<Uri>(), It.IsAny<Stream>())).Returns(SaveStatus.Success);

			var status = sut.ConfigureClientWith(new Uri("C:\\TEMP\\Some\\file.seb"), password);

			binarySerializer.Verify(b => b.TrySerialize(
				It.IsAny<Dictionary<string, object>>(),
				It.Is<PublicKeyParameters>(p => p.InnerEncryption == password && p.SymmetricEncryption)), Times.Once);

			Assert.AreEqual(SaveStatus.Success, status);
		}

		[TestMethod]
		public void ConfigureClient_MustAbortProcessOnError()
		{
			var stream = new MemoryStream() as Stream;
			var password = new PasswordParameters { Password = "test123" };
			var parseResult = new ParseResult
			{
				Format = FormatType.Binary,
				RawData = new Dictionary<string, object>(),
				Status = LoadStatus.Success
			};
			var serializeResult = new SerializeResult
			{
				Data = new MemoryStream(),
				Status = SaveStatus.Success
			};

			RegisterModules();

			fileLoader.Setup(n => n.TryLoad(It.IsAny<Uri>(), out stream)).Returns(LoadStatus.Success);
			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Throws<Exception>();

			var status = sut.ConfigureClientWith(new Uri("C:\\TEMP\\Some\\file.seb"), password);

			fileLoader.Verify(n => n.TryLoad(It.IsAny<Uri>(), out stream), Times.Once);
			binaryParser.Verify(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>()), Times.Never);
			certificateStore.Verify(c => c.ExtractAndImportIdentities(It.IsAny<Dictionary<string, object>>()), Times.Never);
			binarySerializer.Verify(b => b.TrySerialize(It.IsAny<Dictionary<string, object>>(), It.IsAny<EncryptionParameters>()), Times.Never);
			fileSaver.Verify(f => f.TrySave(It.IsAny<Uri>(), It.IsAny<Stream>()), Times.Never);

			Assert.AreEqual(SaveStatus.UnexpectedError, status);
		}

		[TestMethod]
		public void TryLoad_MustWorkAsExpected()
		{
			var stream = new MemoryStream() as Stream;
			var parseResult = new ParseResult { RawData = new Dictionary<string, object>(), Status = LoadStatus.Success };

			RegisterModules();

			networkLoader.Setup(n => n.TryLoad(It.IsAny<Uri>(), out stream)).Returns(LoadStatus.Success);
			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Returns(true);
			binaryParser.Setup(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>())).Returns(parseResult);

			var result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			fileLoader.Verify(f => f.CanLoad(It.IsAny<Uri>()), Times.Once);
			fileLoader.Verify(f => f.TryLoad(It.IsAny<Uri>(), out stream), Times.Never);
			networkLoader.Verify(n => n.CanLoad(It.IsAny<Uri>()), Times.Once);
			networkLoader.Verify(n => n.TryLoad(It.IsAny<Uri>(), out stream), Times.Once);
			binaryParser.Verify(b => b.CanParse(It.IsAny<Stream>()), Times.Once);
			binaryParser.Verify(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>()), Times.Once);
			xmlParser.Verify(x => x.CanParse(It.IsAny<Stream>()), Times.AtMostOnce);
			xmlParser.Verify(x => x.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>()), Times.Never);

			Assert.AreEqual(LoadStatus.Success, result);
		}

		[TestMethod]
		public void TryLoad_MustReportPasswordNeed()
		{
			var stream = new MemoryStream() as Stream;
			var parseResult = new ParseResult { Status = LoadStatus.PasswordNeeded };

			RegisterModules();

			networkLoader.Setup(n => n.TryLoad(It.IsAny<Uri>(), out stream)).Returns(LoadStatus.Success);
			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Returns(true);
			binaryParser.Setup(b => b.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>())).Returns(parseResult);

			var result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			Assert.AreEqual(LoadStatus.PasswordNeeded, result);
		}

		[TestMethod]
		public void TryLoad_MustNotFailToIfNoLoaderRegistered()
		{
			var result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			Assert.AreEqual(LoadStatus.NotSupported, result);

			sut.Register(fileLoader.Object);
			sut.Register(networkLoader.Object);

			result = sut.TryLoadSettings(new Uri("ftp://www.blubb.org"), out _);

			fileLoader.Verify(f => f.CanLoad(It.IsAny<Uri>()), Times.Once);
			networkLoader.Verify(n => n.CanLoad(It.IsAny<Uri>()), Times.Once);

			Assert.AreEqual(LoadStatus.NotSupported, result);
		}

		[TestMethod]
		public void TryLoad_MustNotFailIfNoParserRegistered()
		{
			var data = default(Stream);

			networkLoader.Setup(l => l.TryLoad(It.IsAny<Uri>(), out data)).Returns(LoadStatus.Success);
			sut.Register(networkLoader.Object);

			var result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			networkLoader.Verify(n => n.TryLoad(It.IsAny<Uri>(), out data), Times.Once);

			Assert.AreEqual(LoadStatus.NotSupported, result);
		}

		[TestMethod]
		public void TryLoad_MustNotFailInCaseOfUnexpectedError()
		{
			var data = default(Stream);

			networkLoader.Setup(l => l.TryLoad(It.IsAny<Uri>(), out data)).Throws<Exception>();
			sut.Register(networkLoader.Object);

			var result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			Assert.AreEqual(LoadStatus.UnexpectedError, result);

			binaryParser.Setup(b => b.CanParse(It.IsAny<Stream>())).Throws<Exception>();
			networkLoader.Setup(l => l.TryLoad(It.IsAny<Uri>(), out data)).Returns(LoadStatus.Success);
			sut.Register(binaryParser.Object);

			result = sut.TryLoadSettings(new Uri("http://www.blubb.org"), out _);

			Assert.AreEqual(LoadStatus.UnexpectedError, result);
		}

		[TestMethod]
		public void MustInitializeSessionConfiguration()
		{
			var appConfig = sut.InitializeAppConfig();
			var configuration = sut.InitializeSessionConfiguration();

			Assert.IsNull(configuration.Settings);
			Assert.IsInstanceOfType(configuration.AppConfig, typeof(AppConfig));
			Assert.IsInstanceOfType(configuration.ClientAuthenticationToken, typeof(Guid));
			Assert.IsInstanceOfType(configuration.SessionId, typeof(Guid));
		}

		[TestMethod]
		public void MustUpdateAppConfig()
		{
			var appConfig = sut.InitializeAppConfig();
			var clientAddress = appConfig.ClientAddress;
			var clientId = appConfig.ClientId;
			var clientLogFilePath = appConfig.ClientLogFilePath;
			var runtimeAddress = appConfig.RuntimeAddress;
			var runtimeId = appConfig.RuntimeId;
			var runtimeLogFilePath = appConfig.RuntimeLogFilePath;
			var serviceEventName = appConfig.ServiceEventName;

			var configuration = sut.InitializeSessionConfiguration();

			Assert.AreEqual(configuration.AppConfig.ClientLogFilePath, clientLogFilePath);
			Assert.AreEqual(configuration.AppConfig.RuntimeAddress, runtimeAddress);
			Assert.AreEqual(configuration.AppConfig.RuntimeId, runtimeId);
			Assert.AreEqual(configuration.AppConfig.RuntimeLogFilePath, runtimeLogFilePath);

			Assert.AreNotEqual(configuration.AppConfig.ClientAddress, clientAddress);
			Assert.AreNotEqual(configuration.AppConfig.ClientId, clientId);
			Assert.AreNotEqual(configuration.AppConfig.ServiceEventName, serviceEventName);
		}

		[TestMethod]
		public void MustUpdateSessionConfiguration()
		{
			var appConfig = sut.InitializeAppConfig();
			var firstSession = sut.InitializeSessionConfiguration();
			var secondSession = sut.InitializeSessionConfiguration();
			var thirdSession = sut.InitializeSessionConfiguration();

			Assert.AreNotEqual(firstSession.SessionId, secondSession.SessionId);
			Assert.AreNotEqual(firstSession.ClientAuthenticationToken, secondSession.ClientAuthenticationToken);
			Assert.AreNotEqual(secondSession.SessionId, thirdSession.SessionId);
			Assert.AreNotEqual(secondSession.ClientAuthenticationToken, thirdSession.ClientAuthenticationToken);
		}

		private void RegisterModules()
		{
			sut.Register(binaryParser.Object);
			sut.Register(binarySerializer.Object);
			sut.Register(fileLoader.Object);
			sut.Register(fileSaver.Object);
			sut.Register(networkLoader.Object);
			sut.Register(xmlParser.Object);
			sut.Register(xmlSerializer.Object);
		}

		/// <summary>
		/// Required for unit tests to be able to retrieve the <see cref="Assembly.GetEntryAssembly"/> while executing.
		/// </summary>
		public void SetEntryAssembly()
		{
			var assembly = Assembly.GetCallingAssembly();
			var manager = new AppDomainManager();
			var entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);

			entryAssemblyfield.SetValue(manager, assembly);

			var domain = AppDomain.CurrentDomain;
			var domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);

			domainManagerField.SetValue(domain, manager);
		}
	}
}
