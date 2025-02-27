/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
		private const string DATA_FILE_EXTENSION = "spsdat";
		private const string IMAGE_FILE_EXTENSION = "spsimg";

		private readonly AppConfig appConfig;
		private readonly Lazy<string> directory;
		private readonly Encryptor encryptor;
		private readonly ILogger logger;
		private readonly ScreenProctoringSettings settings;
		private readonly ConcurrentQueue<(string fileName, int checksum, string hash)> queue;

		internal int Count => queue.Count;
		internal string Directory => directory.Value;
		internal long Size { get; private set; }

		public Cache(AppConfig appConfig, ILogger logger, ScreenProctoringSettings settings)
		{
			this.appConfig = appConfig;
			this.directory = new Lazy<string>(InitializeDirectory);
			this.encryptor = new Encryptor(settings);
			this.logger = logger;
			this.settings = settings;
			this.queue = new ConcurrentQueue<(string, int, string)>();
		}

		internal bool Any()
		{
			return queue.Any();
		}

		internal void Clear()
		{
			while (queue.Any())
			{
				if (queue.TryDequeue(out var item))
				{
					var (dataPath, imagePath) = BuildPathsFor(item.fileName);

					File.Delete(dataPath);
					File.Delete(imagePath);
				}
			}

			Size = 0;

			logger.Debug("Cleared all data.");
		}

		internal bool TryEnqueue(MetaData metaData, ScreenShot screenShot)
		{
			var fileName = $"{screenShot.CaptureTime:yyyy-MM-dd HH\\hmm\\mss\\sfff\\m\\s}";
			var success = false;

			try
			{
				var (dataPath, imagePath) = BuildPathsFor(fileName);

				if (Size <= settings.CacheSize * 1000000)
				{
					SaveData(dataPath, metaData, screenShot);
					SaveImage(imagePath, screenShot);
					Enqueue(fileName, metaData, screenShot);

					logger.Debug($"Cached data for '{fileName}', now holding {Count} item(s) counting {Size / 1000000.0:N1} MB.");
				}
				else
				{
					logger.Debug($"The maximum cache size of {settings.CacheSize:N1} MB has been reached, dropping data for '{fileName}'!");
				}

				success = true;
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
					var (dataPath, imagePath) = BuildPathsFor(item.fileName);

					LoadData(dataPath, out metaData, out screenShot);
					LoadImage(imagePath, screenShot);
					Dequeue(item.checksum, dataPath, item.fileName, item.hash, imagePath, metaData, screenShot);

					success = true;

					logger.Debug($"Removed data for '{item.fileName}', {Count} item(s) counting {Size / 1000000.0:N1} MB remaining.");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to remove data for '{item.fileName}'!", e);
				}
			}

			return success;
		}

		private (string dataPath, string imagePath) BuildPathsFor(string fileName)
		{
			var dataPath = Path.Combine(Directory, $"{fileName}.{DATA_FILE_EXTENSION}");
			var imagePath = Path.Combine(Directory, $"{fileName}.{IMAGE_FILE_EXTENSION}");

			return (dataPath, imagePath);
		}

		private void Dequeue(int checksum, string dataPath, string fileName, string hash, string imagePath, MetaData metaData, ScreenShot screenShot)
		{
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

		private string InitializeDirectory()
		{
			var path = Path.Combine(appConfig.TemporaryDirectory, nameof(ScreenProctoring));

			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
				logger.Debug($"Created caching directory '{path}'.");
			}

			return path;
		}

		private void LoadData(string filePath, out MetaData metaData, out ScreenShot screenShot)
		{
			var encrypted = File.ReadAllBytes(filePath);
			var data = encryptor.Decrypt(encrypted);
			var text = Encoding.UTF8.GetString(data);
			var document = XDocument.Parse(text);
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

			Size -= encrypted.Length;
		}

		private void LoadImage(string filePath, ScreenShot screenShot)
		{
			var encrypted = File.ReadAllBytes(filePath);
			var image = encryptor.Decrypt(encrypted);

			screenShot.Data = image;
			Size -= encrypted.Length;
		}

		private void SaveData(string filePath, MetaData metaData, ScreenShot screenShot)
		{
			var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			xml.Add(
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

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
			{
				xml.Save(writer);

				var data = stream.ToArray();
				var encrypted = encryptor.Encrypt(data);

				File.WriteAllBytes(filePath, encrypted);
				Size += encrypted.Length;
			}
		}

		private void SaveImage(string filePath, ScreenShot screenShot)
		{
			var encrypted = encryptor.Encrypt(screenShot.Data);

			File.WriteAllBytes(filePath, encrypted);
			Size += encrypted.Length;
		}
	}
}
