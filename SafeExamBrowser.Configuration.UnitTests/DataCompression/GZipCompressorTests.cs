/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.DataCompression;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.DataCompression
{
	[TestClass]
	public class GZipCompressorTests
	{
		private const int ID1 = 0x1F;
		private const int ID2 = 0x8B;
		private const int CM = 8;
		private const int FOOTER_LENGTH = 8;
		private const int HEADER_LENGTH = 10;

		private Mock<ILogger> logger;
		private GZipCompressor sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new GZipCompressor(logger.Object);
		}

		[TestMethod]
		public void MustCorrectlyDetectGZipStream()
		{
			var randomBytes = new byte[123];

			new Random().NextBytes(randomBytes);

			Assert.IsFalse(sut.IsCompressed(null));
			Assert.IsFalse(sut.IsCompressed(new MemoryStream()));
			Assert.IsFalse(sut.IsCompressed(new MemoryStream(randomBytes)));

			Assert.IsTrue(sut.IsCompressed(new MemoryStream(new byte[] { ID1, ID2, CM }.Concat(randomBytes).ToArray())));
		}

		[TestMethod]
		public void MustPerformCorrectly()
		{
			var data = Encoding.UTF8.GetBytes(String.Join(" ", Enumerable.Repeat("A comparatively easy text to compress.", 100)));
			var compressed = sut.Compress(new MemoryStream(data));

			compressed.Seek(0, SeekOrigin.Begin);

			Assert.AreEqual(ID1, compressed.ReadByte());
			Assert.AreEqual(ID2, compressed.ReadByte());
			Assert.AreEqual(CM, compressed.ReadByte());
			Assert.IsTrue(compressed.Length < data.Length);

			var decompressed = sut.Decompress(compressed);

			decompressed.Seek(0, SeekOrigin.Begin);

			foreach (var item in data)
			{
				Assert.AreEqual(item, decompressed.ReadByte());
			}

			Assert.IsTrue(decompressed.Length == data.Length);
		}

		[TestMethod]
		public void MustPreviewDataCorrectly()
		{
			var data = Encoding.UTF8.GetBytes("A comparatively easy text to compress.");
			var compressed = sut.Compress(new MemoryStream(data));

			var preview = sut.Peek(compressed, 5);

			try
			{
				var position = compressed.Position;
				var length = compressed.Length;
			}
			catch (ObjectDisposedException)
			{
				Assert.Fail("Source stream was disposed after previewing data!");
			}

			Assert.AreEqual(5, preview.Length);
			Assert.IsTrue(Encoding.UTF8.GetBytes("A com").SequenceEqual(preview));
		}
	}
}
