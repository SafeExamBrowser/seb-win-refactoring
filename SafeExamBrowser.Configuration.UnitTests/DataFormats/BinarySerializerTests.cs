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
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataCompression;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.DataFormats
{
	[TestClass]
	public class BinarySerializerTests
	{
		private Mock<IDataCompressor> compressor;
		private Mock<ILogger> logger;
		private Mock<IPasswordEncryption> passwordEncryption;
		private Mock<IPublicKeyEncryption> publicKeyEncryption;
		private Mock<IPublicKeyEncryption> symmetricEncryption;
		private Mock<IDataSerializer> xmlSerializer;
		private SerializeResult xmlResult;

		private BinarySerializer sut;

		[TestInitialize]
		public void Initialize()
		{
			compressor = new Mock<IDataCompressor>();
			logger = new Mock<ILogger>();
			passwordEncryption = new Mock<IPasswordEncryption>();
			publicKeyEncryption = new Mock<IPublicKeyEncryption>();
			symmetricEncryption = new Mock<IPublicKeyEncryption>();
			xmlResult = new SerializeResult { Data = new MemoryStream(), Status = SaveStatus.Success };
			xmlSerializer = new Mock<IDataSerializer>();

			compressor.Setup(c => c.Compress(It.IsAny<Stream>())).Returns(new MemoryStream());
			xmlSerializer.Setup(x => x.TrySerialize(It.IsAny<IDictionary<string, object>>(), It.IsAny<EncryptionParameters>())).Returns(xmlResult);

			sut = new BinarySerializer(compressor.Object, logger.Object, passwordEncryption.Object, publicKeyEncryption.Object, symmetricEncryption.Object, xmlSerializer.Object);
		}

		[TestMethod]
		public void MustOnlySupportBinaryFormat()
		{
			var values = Enum.GetValues(typeof(FormatType));

			foreach (var value in values)
			{
				if (value is FormatType format && format != FormatType.Binary)
				{
					Assert.IsFalse(sut.CanSerialize(format));
				}
			}

			Assert.IsTrue(sut.CanSerialize(FormatType.Binary));
		}

		[TestMethod]
		public void MustCorrectlySerializePlainDataBlock()
		{
			var data = new Dictionary<string, object>();
			var result = sut.TrySerialize(data);

			compressor.Verify(c => c.Compress(It.IsAny<Stream>()), Times.Exactly(2));
			xmlSerializer.Verify(x => x.TrySerialize(It.Is<IDictionary<string, object>>(d => d == data), It.Is<EncryptionParameters>(e => e == null)), Times.Once);
			passwordEncryption.VerifyNoOtherCalls();
			publicKeyEncryption.VerifyNoOtherCalls();
			symmetricEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(SaveStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustCorrectlySerializePasswordBlock()
		{
			var encrypted = new MemoryStream() as Stream;
			var data = new Dictionary<string, object>();
			var encryption = new PasswordParameters { Password = "blubb" };

			passwordEncryption.Setup(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<string>(), out encrypted)).Returns(SaveStatus.Success);

			var result = sut.TrySerialize(data, encryption);

			compressor.Verify(c => c.Compress(It.IsAny<Stream>()), Times.Exactly(2));
			passwordEncryption.Verify(p => p.Encrypt(It.IsAny<Stream>(), It.Is<string>(s => s == encryption.Password), out encrypted), Times.Once);
			xmlSerializer.Verify(x => x.TrySerialize(It.Is<IDictionary<string, object>>(d => d == data), It.Is<EncryptionParameters>(e => e == null)), Times.Once);
			publicKeyEncryption.VerifyNoOtherCalls();
			symmetricEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(SaveStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustCorrectlySerializePublicKeyBlock()
		{
			var encrypted = new MemoryStream() as Stream;
			var data = new Dictionary<string, object>();
			var encryption = new PublicKeyParameters
			{
				InnerEncryption = new PasswordParameters { Password = "test" },
				SymmetricEncryption = false
			};

			passwordEncryption.Setup(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<string>(), out encrypted)).Returns(SaveStatus.Success);
			publicKeyEncryption.Setup(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<X509Certificate2>(), out encrypted)).Returns(SaveStatus.Success);

			var result = sut.TrySerialize(data, encryption);

			compressor.Verify(c => c.Compress(It.IsAny<Stream>()), Times.Exactly(2));
			passwordEncryption.Verify(p => p.Encrypt(It.IsAny<Stream>(), It.Is<string>(s => s == encryption.InnerEncryption.Password), out encrypted), Times.Once);
			publicKeyEncryption.Verify(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<X509Certificate2>(), out encrypted), Times.Once);
			xmlSerializer.Verify(x => x.TrySerialize(It.Is<IDictionary<string, object>>(d => d == data), It.Is<EncryptionParameters>(e => e == null)), Times.Once);
			symmetricEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(SaveStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustCorrectlySerializePublicKeySymmetricBlock()
		{
			var encrypted = new MemoryStream() as Stream;
			var data = new Dictionary<string, object>();
			var encryption = new PublicKeyParameters
			{
				SymmetricEncryption = true
			};

			symmetricEncryption.Setup(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<X509Certificate2>(), out encrypted)).Returns(SaveStatus.Success);

			var result = sut.TrySerialize(data, encryption);

			compressor.Verify(c => c.Compress(It.IsAny<Stream>()), Times.Exactly(2));
			symmetricEncryption.Verify(p => p.Encrypt(It.IsAny<Stream>(), It.IsAny<X509Certificate2>(), out encrypted), Times.Once);
			xmlSerializer.Verify(x => x.TrySerialize(It.Is<IDictionary<string, object>>(d => d == data), It.Is<EncryptionParameters>(e => e == null)), Times.Once);
			passwordEncryption.VerifyNoOtherCalls();
			publicKeyEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(SaveStatus.Success, result.Status);
		}
	}
}
