/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SafeExamBrowser.Configuration.UnitTests
{
	[TestClass]
	public class SubStreamTests
	{
		private Mock<Stream> stream;

		[TestInitialize]
		public void Initialize()
		{
			stream = new Mock<Stream>();

			stream.SetupGet(s => s.CanRead).Returns(true);
			stream.SetupGet(s => s.CanSeek).Returns(true);
			stream.SetupGet(s => s.Length).Returns(1000);
		}

		[TestMethod]
		public void MustSetPropertiesCorrectly()
		{
			var sut = new SubStream(stream.Object, 100, 200);

			Assert.IsTrue(sut.CanRead);
			Assert.IsTrue(sut.CanSeek);
			Assert.IsFalse(sut.CanWrite);
			Assert.AreEqual(200, sut.Length);
			Assert.AreEqual(0, sut.Position);
		}

		[TestMethod]
		public void MustReadCorrectly()
		{
			var position = 750L;
			var sut = new SubStream(stream.Object, 100, 200);

			stream.SetupGet(s => s.Position).Returns(position);
			stream.SetupSet(s => s.Position = It.IsAny<long>()).Callback<long>(p => position = p);
			stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns<byte[], int, int>((a, o, c) => c);
			sut.Position = 50;

			var bytesRead = sut.Read(new byte[25], 0, 25);

			stream.Verify(s => s.Read(It.IsAny<byte[]>(), 0, 25), Times.Once);

			Assert.AreEqual(25, bytesRead);
			Assert.AreEqual(75, sut.Position);
			Assert.AreEqual(750, position);

			sut.Position = 150;
			bytesRead = sut.Read(new byte[75], 0, 75);

			stream.Verify(s => s.Read(It.IsAny<byte[]>(), 0, 50), Times.Once);

			Assert.AreEqual(50, bytesRead);
			Assert.AreEqual(200, sut.Position);
			Assert.AreEqual(750, position);
		}

		[TestMethod]
		public void MustNotReadOutsideOfBounds()
		{
			var sut = new SubStream(stream.Object, 100, 200);

			sut.Position = -1;

			var bytesRead = sut.Read(new byte[0], 0, 0);

			Assert.AreEqual(0, bytesRead);
			stream.Verify(s => s.ReadByte(), Times.Never);
			stream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

			sut.Position = 500;
			bytesRead = sut.Read(new byte[0], 0, 0);

			Assert.AreEqual(0, bytesRead);
			stream.Verify(s => s.ReadByte(), Times.Never);
			stream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}

		[TestMethod]
		public void MustReadByteCorrectly()
		{
			var sut = new SubStream(stream.Object, 100, 200);

			sut.Position = -100;
			Assert.AreEqual(-1, sut.ReadByte());

			sut.Position = 200;
			Assert.AreEqual(-1, sut.ReadByte());

			sut.Position = 25;
			sut.ReadByte();

			stream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
		}

		[TestMethod]
		public void MustSeekCorrectly()
		{
			var sut = new SubStream(stream.Object, 100, 200);

			sut.Seek(10, SeekOrigin.Begin);
			Assert.AreEqual(10, sut.Position);

			sut.Seek(15, SeekOrigin.Current);
			Assert.AreEqual(25, sut.Position);

			sut.Seek(-5, SeekOrigin.Current);
			Assert.AreEqual(20, sut.Position);

			sut.Seek(-50, SeekOrigin.End);
			Assert.AreEqual(150, sut.Position);

			sut.Seek(10, SeekOrigin.End);
			Assert.AreEqual(210, sut.Position);

			sut.Seek(-10, SeekOrigin.Begin);
			Assert.AreEqual(-10, sut.Position);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustNotAllowNonReadableStream()
		{
			stream.SetupGet(s => s.CanRead).Returns(false);

			new SubStream(stream.Object, 0, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustNotAllowNonSeekableStream()
		{
			stream.SetupGet(s => s.CanSeek).Returns(false);

			new SubStream(stream.Object, 0, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MustNotAllowOffsetSmallerThanZero()
		{
			new SubStream(stream.Object, -1, 100);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MustNotAllowLengthSmallerThanOne()
		{
			new SubStream(stream.Object, 100, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void MustNotSupportFlushing()
		{
			new SubStream(stream.Object, 100, 100).Flush();
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void MustNotSupportChangingLength()
		{
			new SubStream(stream.Object, 100, 100).SetLength(100);
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void MustNotSupportWriting()
		{
			new SubStream(stream.Object, 100, 100).Write(new byte[0], 0, 0);
		}
	}
}
