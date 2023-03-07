/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.Cryptography
{
	[TestClass]
	public class PasswordEncryptionTests
	{
		private Mock<ILogger> logger;
		private PasswordEncryption sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new PasswordEncryption(logger.Object);
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var password = "test1234";
			var message = Encoding.UTF8.GetBytes("A super secret message!");
			var saveStatus = sut.Encrypt(new MemoryStream(message), password, out var encrypted);
			var loadStatus = sut.Decrypt(encrypted, password, out var decrypted);
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
		public void MustRequestPasswordForDecryption()
		{
			var status = sut.Decrypt(new MemoryStream(), null, out _);

			Assert.AreEqual(LoadStatus.PasswordNeeded, status);
		}

		[TestMethod]
		public void MustRequestPasswordIfInvalid()
		{
			var password = "test1234";
			var saveStatus = sut.Encrypt(new MemoryStream(Encoding.UTF8.GetBytes("A super secret message!")), password, out var encrypted);
			var loadStatus = sut.Decrypt(encrypted, "not the correct password", out _);

			Assert.AreEqual(SaveStatus.Success, saveStatus);
			Assert.AreEqual(LoadStatus.PasswordNeeded, loadStatus);
		}
	}
}
