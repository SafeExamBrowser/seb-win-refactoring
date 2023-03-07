/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;

namespace SafeExamBrowser.Configuration
{
	/// <summary>
	/// A read-only wrapper for a subsection of another, larger stream.
	/// </summary>
	internal class SubStream : Stream
	{
		private long length;
		private long offset;
		private Stream original;

		public override bool CanRead => original.CanRead;
		public override bool CanSeek => original.CanSeek;
		public override bool CanWrite => false;
		public override long Length => length;
		public override long Position { get; set; }

		/// <summary>
		/// Creates a new wrapper for the specified subsection of the given stream.
		/// </summary>
		/// <remarks>
		/// 
		/// Below an example of a subsection within a stream:
		/// 
		/// +==============+==============================================================+==============================+
		/// |      ...     |####################### subsection ###########################|              ...             |
		/// +==============+==============================================================+==============================+
		/// ^              ^                                                              ^                              ^
		/// |              |                                                              |                              |
		/// |              + offset                                                       + length                       |
		/// |                                                                                                            |
		/// + start of original                                                                          end of original +
		/// 
		/// </remarks>
		/// <exception cref="ArgumentException">In case the original stream does not support <see cref="Stream.CanRead"/>.</exception>
		/// <exception cref="ArgumentException">In case the original stream does not support <see cref="Stream.CanSeek"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">In case the specified subsection is outside the bounds of the original stream.</exception>
		public SubStream(Stream original, long offset, long length)
		{
			this.original = original;
			this.offset = offset;
			this.length = length;

			if (!original.CanRead)
			{
				throw new ArgumentException("The original stream must support reading!", nameof(original));
			}

			if (!original.CanSeek)
			{
				throw new ArgumentException("The original stream must support seeking!", nameof(original));
			}

			if (original.Length < offset + length || offset < 0 || length < 1)
			{
				throw new ArgumentOutOfRangeException($"Specified subsection is outside the bounds of the original stream!");
			}
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var originalPosition = original.Position;

			if (Position < 0 || Position >= Length)
			{
				return 0;
			}

			if (Position + count >= Length)
			{
				count = Convert.ToInt32(Length - Position);
			}

			original.Seek(this.offset + Position, SeekOrigin.Begin);

			var bytesRead = original.Read(buffer, offset, count);

			Position += bytesRead;
			original.Seek(originalPosition, SeekOrigin.Begin);

			return bytesRead;
		}

		public override int ReadByte()
		{
			if (Position < 0 || Position >= Length)
			{
				return -1;
			}

			return base.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position += offset;
					break;
				case SeekOrigin.End:
					Position = length + offset;
					break;
				default:
					throw new NotImplementedException($"Seeking from position '{origin}' is not implemented!");
			}

			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
