/*
 * Copyright (c) 2023 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class Cache
	{
		private const string DATA_FILE_EXTENSION = "xml";

		private readonly AppConfig appConfig;
		private readonly ILogger logger;
		private readonly ConcurrentQueue<(string fileName, int checksum, string hash)> queue;

		internal int Count => queue.Count;
		internal string Directory { get; private set; }

		public Cache(AppConfig appConfig, ILogger logger)
		{
			this.appConfig = appConfig;
			this.logger = logger;
			this.queue = new ConcurrentQueue<(string, int, string)>();
		}

		internal bool Any()
		{
			return queue.Any();
		}

		internal bool TryEnqueue(MetaData metaData, ScreenShot screenShot)
		{
			var fileName = $"{screenShot.CaptureTime:yyyy-MM-dd HH\\hmm\\mss\\sfff\\m\\s}";
			var success = false;

			try
			{
				InitializeDirectory();
				SaveData(fileName, metaData, screenShot);
				SaveImage(fileName, screenShot);
				Enqueue(fileName, metaData, screenShot);

				success = true;

				logger.Debug($"Cached data for '{fileName}', now holding {Count} item(s).");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to cache data for '{fileName}'!", e);
			}

			return success;
		}

		internal bool TryDequeue(out MetaData metaData, out ScreenShot screenShot)
		{
			var success = false;

			metaData = default;
			screenShot = default;

			if (queue.Any() && queue.TryPeek(out var item))
			{
				try
				{
					LoadData(item.fileName, out metaData, out screenShot);
					LoadImage(item.fileName, screenShot);
					Dequeue(item.fileName, item.checksum, item.hash, metaData, screenShot);

					success = true;

					logger.Debug($"Removed data for '{item.fileName}', {Count} item(s) remaining.");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to remove data for '{item.fileName}'!", e);
				}
			}

			return success;
		}

		private void Dequeue(string fileName, int checksum, string hash, MetaData metaData, ScreenShot screenShot)
		{
			var dataPath = Path.Combine(Directory, $"{fileName}.{DATA_FILE_EXTENSION}");
			var extension = screenShot.Format.ToString().ToLower();
			var imagePath = Path.Combine(Directory, $"{fileName}.{extension}");

			if (checksum != GenerateChecksum(screenShot))
			{
				logger.Warn($"The checksum for '{fileName}' does not match, the image data may be manipulated!");
			}

			if (hash != GenerateHash(metaData, screenShot))
			{
				logger.Warn($"The hash for '{fileName}' does not match, the metadata may be manipulated!");
			}

			File.Delete(dataPath);
			File.Delete(imagePath);

			while (!queue.TryDequeue(out _)) ;
		}

		private void Enqueue(string fileName, MetaData metaData, ScreenShot screenShot)
		{
			var checksum = GenerateChecksum(screenShot);
			var hash = GenerateHash(metaData, screenShot);

			queue.Enqueue((fileName, checksum, hash));
		}

		private int GenerateChecksum(ScreenShot screenShot)
		{
			var checksum = default(int);

			foreach (var data in screenShot.Data)
			{
				unchecked
				{
					checksum += data;
				}
			}

			return checksum;
		}

		private string GenerateHash(MetaData metaData, ScreenShot screenShot)
		{
			var hash = default(string);

			using (var algorithm = new SHA256Managed())
			{
				var input = metaData.ToJson() + screenShot.CaptureTime + screenShot.Format + screenShot.Height + screenShot.Width;
				var bytes = Encoding.UTF8.GetBytes(input);
				var result = algorithm.ComputeHash(bytes);

				hash = string.Join(string.Empty, result.Select(b => $"{b:x2}"));
			}

			return hash;
		}

		private void InitializeDirectory()
		{
			if (Directory == default)
			{
				Directory = Path.Combine(appConfig.TemporaryDirectory, nameof(ScreenProctoring));
			}

			if (!System.IO.Directory.Exists(Directory))
			{
				System.IO.Directory.CreateDirectory(Directory);
				logger.Debug($"Created caching directory '{Directory}'.");
			}
		}

		private void LoadData(string fileName, out MetaData metaData, out ScreenShot screenShot)
		{
			var dataPath = Path.Combine(Directory, $"{fileName}.{DATA_FILE_EXTENSION}");
			var document = XDocument.Load(dataPath);
			var xml = document.Descendants(nameof(MetaData)).First();

			metaData = new MetaData();
			screenShot = new ScreenShot();

			metaData.ApplicationInfo = xml.Descendants(nameof(MetaData.ApplicationInfo)).First().Value;
			metaData.BrowserInfo = xml.Descendants(nameof(MetaData.BrowserInfo)).First().Value;
			metaData.Elapsed = TimeSpan.Parse(xml.Descendants(nameof(MetaData.Elapsed)).First().Value);
			metaData.TriggerInfo = xml.Descendants(nameof(MetaData.TriggerInfo)).First().Value;
			metaData.Urls = xml.Descendants(nameof(MetaData.Urls)).First().Value;
			metaData.WindowTitle = xml.Descendants(nameof(MetaData.WindowTitle)).First().Value;

			xml = document.Descendants(nameof(ScreenShot)).First();

			screenShot.CaptureTime = DateTime.Parse(xml.Descendants(nameof(ScreenShot.CaptureTime)).First().Value);
			screenShot.Format = (ImageFormat) Enum.Parse(typeof(ImageFormat), xml.Descendants(nameof(ScreenShot.Format)).First().Value);
			screenShot.Height = int.Parse(xml.Descendants(nameof(ScreenShot.Height)).First().Value);
			screenShot.Width = int.Parse(xml.Descendants(nameof(ScreenShot.Width)).First().Value);
		}

		private void LoadImage(string fileName, ScreenShot screenShot)
		{
			var extension = screenShot.Format.ToString().ToLower();
			var imagePath = Path.Combine(Directory, $"{fileName}.{extension}");

			screenShot.Data = File.ReadAllBytes(imagePath);
		}

		private void SaveData(string fileName, MetaData metaData, ScreenShot screenShot)
		{
			var data = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			var dataPath = Path.Combine(Directory, $"{fileName}.{DATA_FILE_EXTENSION}");

			data.Add(
				new XElement("Data",
					new XElement(nameof(MetaData),
						new XElement(nameof(MetaData.ApplicationInfo), metaData.ApplicationInfo),
						new XElement(nameof(MetaData.BrowserInfo), metaData.BrowserInfo),
						new XElement(nameof(MetaData.Elapsed), metaData.Elapsed.ToString()),
						new XElement(nameof(MetaData.TriggerInfo), metaData.TriggerInfo),
						new XElement(nameof(MetaData.Urls), metaData.Urls),
						new XElement(nameof(MetaData.WindowTitle), metaData.WindowTitle)
					),
					new XElement(nameof(ScreenShot),
						new XElement(nameof(ScreenShot.CaptureTime), screenShot.CaptureTime.ToString()),
						new XElement(nameof(ScreenShot.Format), screenShot.Format),
						new XElement(nameof(ScreenShot.Height), screenShot.Height),
						new XElement(nameof(ScreenShot.Width), screenShot.Width)
					)
				)
			);

			data.Save(dataPath);
		}

		private void SaveImage(string fileName, ScreenShot screenShot)
		{
			var extension = screenShot.Format.ToString().ToLower();
			var imagePath = Path.Combine(Directory, $"{fileName}.{extension}");

			File.WriteAllBytes(imagePath, screenShot.Data);
		}
	}
}
