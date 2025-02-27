/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
	internal class ScreenShotProcessor : IDisposable
	{
		private readonly ILogger logger;
		private readonly ScreenProctoringSettings settings;

		private Bitmap bitmap;
		private DateTime captureTime;
		private byte[] data;
		private ImageFormat format;
		private int height;
		private int width;

		internal ScreenShot Data => new ScreenShot
		{
			CaptureTime = captureTime,
			Data = data,
			Format = format,
			Height = height,
			Width = width
		};

		public ScreenShotProcessor(ILogger logger, ScreenProctoringSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public void Dispose()
		{
			bitmap?.Dispose();
			bitmap = default;
			data = default;
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

			bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			captureTime = DateTime.Now;
			format = settings.ImageFormat;
			this.height = height;
			this.width = width;

			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
				graphics.DrawCursorPosition();
			}

			Serialize();

			logger.Debug($"Captured '{ToReducedString()}' at {captureTime}.");
		}

		private void Downscale()
		{
			if (settings.ImageDownscaling > 1)
			{
				height = Convert.ToInt32(height / settings.ImageDownscaling);
				width = Convert.ToInt32(width / settings.ImageDownscaling);

				var downscaled = new Bitmap(width, height, bitmap.PixelFormat);

				bitmap.DrawInto(downscaled, new Rectangle(0, 0, width, height), ScalingMode.NearestNeighbor);
				bitmap.Dispose();
				bitmap = downscaled;
			}
		}

		private void Quantize()
		{
			var ditherer = settings.ImageDownscaling > 1 ? OrderedDitherer.Bayer2x2 : default;
			var pixelFormat = settings.ImageQuantization.ToPixelFormat();
			var quantizer = settings.ImageQuantization.ToQuantizer();

			bitmap = bitmap.ConvertPixelFormat(pixelFormat, quantizer, ditherer);
		}

		private void Serialize()
		{
			using (var memoryStream = new MemoryStream())
			{
				if (format == ImageFormat.Jpg)
				{
					SerializeJpg(memoryStream);
				}
				else
				{
					bitmap.Save(memoryStream, format.ToSystemFormat());
				}

				data = memoryStream.ToArray();
			}
		}

		private void SerializeJpg(MemoryStream memoryStream)
		{
			var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
			var parameters = new EncoderParameters(1);
			var quality = 100;

			parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
			bitmap.Save(memoryStream, codec, parameters);
		}

		private string ToReducedString()
		{
			return $"{width}x{height}, {data.Length / 1000:N0}kB, {format.ToString().ToUpper()}";
		}
	}
}
