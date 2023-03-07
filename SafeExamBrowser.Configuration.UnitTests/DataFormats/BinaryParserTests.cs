/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
	public class BinaryParserTests
	{
		private Mock<IDataCompressor> compressor;
		private Mock<IHashAlgorithm> hashAlgorithm;
		private Mock<ILogger> logger;
		private Mock<IPasswordEncryption> passwordEncryption;
		private Mock<IPublicKeyEncryption> publicKeyEncryption;
		private Mock<IPublicKeyEncryption> symmetricEncryption;
		private Mock<IDataParser> xmlParser;

		private BinaryParser sut;

		[TestInitialize]
		public void Initialize()
		{
			compressor = new Mock<IDataCompressor>();
			hashAlgorithm = new Mock<IHashAlgorithm>();
			logger = new Mock<ILogger>();
			passwordEncryption = new Mock<IPasswordEncryption>();
			publicKeyEncryption = new Mock<IPublicKeyEncryption>();
			symmetricEncryption = new Mock<IPublicKeyEncryption>();
			xmlParser = new Mock<IDataParser>();

			xmlParser.Setup(p => p.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>())).Returns(new ParseResult { Status = LoadStatus.Success });

			sut = new BinaryParser(compressor.Object, hashAlgorithm.Object, logger.Object, passwordEncryption.Object, publicKeyEncryption.Object, symmetricEncryption.Object, xmlParser.Object);
		}

		[TestMethod]
		public void MustCorrectlyDetectValidPrefixes()
		{
			var data = new byte[123];
			var pswd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.Password).Concat(data).ToArray());
			var pwcc = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PasswordConfigureClient).Concat(data).ToArray());
			var plnd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PlainData).Concat(data).ToArray());
			var pkhs = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PublicKey).Concat(data).ToArray());
			var phsk = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PublicKeySymmetric).Concat(data).ToArray());

			Assert.IsFalse(sut.CanParse(null));
			Assert.IsFalse(sut.CanParse(new MemoryStream()));
			Assert.IsFalse(sut.CanParse(new MemoryStream(data)));

			Assert.IsTrue(sut.CanParse(pswd));
			Assert.IsTrue(sut.CanParse(pwcc));
			Assert.IsTrue(sut.CanParse(plnd));
			Assert.IsTrue(sut.CanParse(pkhs));
			Assert.IsTrue(sut.CanParse(phsk));
		}

		[TestMethod]
		public void MustCorrectlyParsePasswordBlock()
		{
			var data = new byte[123];
			var decrypted = default(Stream);
			var pswd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.Password).Concat(data).ToArray()) as Stream;

			passwordEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), It.Is<string>(s => s == "wrong"), out decrypted)).Returns(LoadStatus.PasswordNeeded);
			passwordEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), It.Is<string>(s => s == "correct"), out decrypted)).Returns(LoadStatus.Success);

			var result = sut.TryParse(pswd);

			Assert.AreEqual(LoadStatus.PasswordNeeded, result.Status);

			result = sut.TryParse(pswd, new PasswordParameters { Password = "wrong" });

			Assert.AreEqual(LoadStatus.PasswordNeeded, result.Status);

			result = sut.TryParse(pswd, new PasswordParameters { Password = "correct" });

			passwordEncryption.Verify(p => p.Decrypt(It.IsAny<Stream>(), It.IsAny<string>(), out decrypted), Times.AtLeastOnce);
			xmlParser.Verify(p => p.TryParse(It.Is<Stream>(s => s == decrypted), It.IsAny<PasswordParameters>()), Times.Once);
			publicKeyEncryption.VerifyNoOtherCalls();
			symmetricEncryption.VerifyNoOtherCalls();
		}

		[TestMethod]
		public void MustCorrectlyParsePlainDataBlock()
		{
			var data = new byte[123];
			var plnd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PlainData).Concat(data).ToArray());

			compressor.Setup(c => c.Decompress(It.IsAny<Stream>())).Returns(plnd);
			compressor.Setup(c => c.Peek(It.IsAny<Stream>(), It.IsAny<int>())).Returns(Encoding.UTF8.GetBytes(BinaryBlock.PlainData));
			compressor.Setup(c => c.IsCompressed(It.IsAny<Stream>())).Returns(true);

			var result = sut.TryParse(plnd);

			compressor.Verify(c => c.IsCompressed(It.IsAny<Stream>()), Times.AtLeastOnce);
			compressor.Verify(c => c.Decompress(It.IsAny<Stream>()), Times.AtLeastOnce);
			xmlParser.Verify(x => x.TryParse(It.IsAny<Stream>(), It.IsAny<PasswordParameters>()), Times.Once);
			passwordEncryption.VerifyNoOtherCalls();
			publicKeyEncryption.VerifyNoOtherCalls();
			symmetricEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(LoadStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustCorrectlyParsePublicKeyBlock()
		{
			var data = new byte[123];
			var certificate = default(X509Certificate2);
			var decrypted = default(Stream);
			var pswd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.Password).Concat(data).ToArray()) as Stream;
			var pkhs = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PublicKey).Concat(data).ToArray());

			passwordEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), It.IsAny<string>(), out decrypted)).Returns(LoadStatus.Success);
			publicKeyEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), out pswd, out certificate)).Returns(LoadStatus.Success);

			var result = sut.TryParse(pkhs, new PasswordParameters { Password = "blubb" });

			publicKeyEncryption.Verify(p => p.Decrypt(It.IsAny<Stream>(), out decrypted, out certificate), Times.Once);
			passwordEncryption.Verify(p => p.Decrypt(It.IsAny<Stream>(), It.Is<string>(s => s == "blubb"), out decrypted), Times.Once);
			symmetricEncryption.VerifyNoOtherCalls();

			Assert.AreEqual(LoadStatus.Success, result.Status);
		}

		[TestMethod]
		public void MustOnlyParseIfFormatSupported()
		{
			var data = new byte[123];
			var certificate = default(X509Certificate2);
			var decrypted = default(Stream);
			var pswd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.Password).Concat(data).ToArray()) as Stream;
			var pwcc = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PasswordConfigureClient).Concat(data).ToArray());
			var plnd = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PlainData).Concat(data).ToArray());
			var pkhs = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PublicKey).Concat(data).ToArray());
			var phsk = new MemoryStream(Encoding.UTF8.GetBytes(BinaryBlock.PublicKeySymmetric).Concat(data).ToArray());

			passwordEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), It.IsAny<string>(), out decrypted)).Returns(LoadStatus.Success);
			publicKeyEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), out pswd, out certificate)).Returns(LoadStatus.Success);
			symmetricEncryption.Setup(p => p.Decrypt(It.IsAny<Stream>(), out pswd, out certificate)).Returns(LoadStatus.Success);

			Assert.AreEqual(LoadStatus.InvalidData, sut.TryParse(new MemoryStream(data)).Status);

			Assert.AreEqual(LoadStatus.Success, sut.TryParse(pswd, new PasswordParameters { Password = "blubb" })?.Status);
			Assert.AreEqual(LoadStatus.Success, sut.TryParse(pwcc, new PasswordParameters { Password = "blubb" })?.Status);
			Assert.AreEqual(LoadStatus.Success, sut.TryParse(plnd).Status);
			Assert.AreEqual(LoadStatus.Success, sut.TryParse(pkhs, new PasswordParameters { Password = "blubb" }).Status);
			Assert.AreEqual(LoadStatus.Success, sut.TryParse(phsk, new PasswordParameters { Password = "blubb" }).Status);
		}
	}
}
