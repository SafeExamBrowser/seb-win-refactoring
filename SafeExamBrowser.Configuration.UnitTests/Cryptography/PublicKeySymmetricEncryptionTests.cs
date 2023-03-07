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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.Cryptography
{
	[TestClass]
	public class PublicKeySymmetricEncryptionTests
	{
		private Mock<ILogger> logger;
		private PasswordEncryption passwordEncryption;
		private Mock<ICertificateStore> store;

		private PublicKeySymmetricEncryption sut;
		private X509Certificate2 certificate;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			passwordEncryption = new PasswordEncryption(logger.Object);
			store = new Mock<ICertificateStore>();

			LoadCertificate();
			store.Setup(s => s.TryGetCertificateWith(It.IsAny<byte[]>(), out certificate)).Returns(true);

			sut = new PublicKeySymmetricEncryption(store.Object, logger.Object, passwordEncryption);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var message = Encoding.UTF8.GetBytes("A super secret message!");
			var saveStatus = sut.Encrypt(new MemoryStream(message), certificate, out var encrypted);
			var loadStatus = sut.Decrypt(encrypted, out var decrypted, out _);
			var original = new MemoryStream(message);

			decrypted.Seek(0, SeekOrigin.Begin);
			original.Seek(0, SeekOrigin.Begin);

			while (original.Position < original.Length)
			{
				Assert.AreEqual(original.ReadByte(), decrypted.ReadByte());
			}

			Assert.AreEqual(SaveStatus.Success, saveStatus);
			Assert.AreEqual(LoadStatus.Success, loadStatus);
		}

		[TestMethod]
		public void MustFailIfCertificateNotFound()
		{
			store.Setup(s => s.TryGetCertificateWith(It.IsAny<byte[]>(), out certificate)).Returns(false);

			var buffer = new byte[20];
			new Random().NextBytes(buffer);
			var data = new MemoryStream(buffer);
			var status = sut.Decrypt(data, out _, out _);

			Assert.AreEqual(LoadStatus.InvalidData, status);
		}

		/// <summary>
		/// makecert -sv UnitTestCert.pvk -n "CN=Unit Test Certificate" UnitTestCert.cer -r -pe -sky eXchange
		/// pvk2pfx -pvk UnitTestCert.pvk -spc UnitTestCert.cer -pfx UnitTestCert.pfx -f
		/// </summary>
		private void LoadCertificate()
		{
			var path = $"{nameof(SafeExamBrowser)}.{nameof(Configuration)}.{nameof(UnitTests)}.UnitTestCert.pfx";

			using (var stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(path))
			{
				var data = new byte[stream.Length];

				stream.Read(data, 0, (int)stream.Length);
				certificate = new X509Certificate2(data);
			}
		}
	}
}
