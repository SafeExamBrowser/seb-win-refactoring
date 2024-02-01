/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using ImageFormat = SafeExamBrowser.Settings.Proctoring.ImageFormat;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Imaging
{
	internal class ScreenShot : IDisposable
	{
		private readonly ILogger logger;
		private readonly ScreenProctoringSettings settings;

		internal Bitmap Bitmap { get; private set; }
		internal byte[] Data { get; private set; }
		internal ImageFormat Format { get; private set; }
		internal int Height { get; private set; }
		internal int Width { get; private set; }

		public ScreenShot(ILogger logger, ScreenProctoringSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public void Dispose()
		{
			Bitmap?.Dispose();
			Bitmap = default;
			Data = default;
		}

		public string ToReducedString()
		{
			return $"{Width}x{Height}, {Data.Length / 1000:N0} kB, {Format.ToString().ToUpper()}";
		}

		public override string ToString()
		{
			return $"resolution: {Width}x{Height}, size: {Data.Length / 1000:N0} kB, format: {Format.ToString().ToUpper()}";
		}

		internal void Compress()
		{
			var order = ProcessingOrder.QuantizingDownscaling;
			var original = ToReducedString();
			var parameters = $"{order}, {settings.ImageQuantization}, 1:{settings.ImageDownscaling}";

			switch (order)
			{
				case ProcessingOrder.DownscalingQuantizing:
					Downscale();
					Quantize();
					Serialize();
					break;
				case ProcessingOrder.QuantizingDownscaling:
					Quantize();
					Downscale();
					Serialize();
					break;
			}

			logger.Debug($"Compressed from '{original}' to '{ToReducedString()}' ({parameters}).");
		}

		internal void Take()
		{
			var x = Screen.AllScreens.Min(s => s.Bounds.X);
			var y = Screen.AllScreens.Min(s => s.Bounds.Y);
			var width = Screen.AllScreens.Max(s => s.Bounds.X + s.Bounds.Width) - x;
			var height = Screen.AllScreens.Max(s => s.Bounds.Y + s.Bounds.Height) - y;

			Bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			Format = settings.ImageFormat;
			Height = height;
			Width = width;

			using (var graphics = Graphics.FromImage(Bitmap))
			{
				graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
				graphics.DrawCursorPosition();
			}

			Serialize();
		}

		private void Downscale()
		{
			if (settings.ImageDownscaling > 1)
			{
				Height = Convert.ToInt32(Height / settings.ImageDownscaling);
				Width = Convert.ToInt32(Width / settings.ImageDownscaling);

				var downscaled = new Bitmap(Width, Height, Bitmap.PixelFormat);

				Bitmap.DrawInto(downscaled, new Rectangle(0, 0, Width, Height), ScalingMode.NearestNeighbor);
				Bitmap.Dispose();
				Bitmap = downscaled;
			}
		}

		private void Quantize()
		{
			var ditherer = settings.ImageDownscaling > 1 ? OrderedDitherer.Bayer2x2 : default;
			var pixelFormat = settings.ImageQuantization.ToPixelFormat();
			var quantizer = settings.ImageQuantization.ToQuantizer();

			Bitmap = Bitmap.ConvertPixelFormat(pixelFormat, quantizer, ditherer);
		}

		private void Serialize()
		{
			using (var memoryStream = new MemoryStream())
			{
				if (Format == ImageFormat.Jpg)
				{
					SerializeJpg(memoryStream);
				}
				else
				{
					Bitmap.Save(memoryStream, Format.ToSystemFormat());
				}

				Data = memoryStream.ToArray();
			}
		}

		private void SerializeJpg(MemoryStream memoryStream)
		{
			var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
			var parameters = new EncoderParameters(1);
			var quality = 100;

			parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
			Bitmap.Save(memoryStream, codec, parameters);
		}
	}
}
