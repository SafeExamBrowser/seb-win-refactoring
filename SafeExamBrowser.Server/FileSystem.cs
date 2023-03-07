/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Server
{
	internal class FileSystem
	{
		private readonly AppConfig appConfig;
		private readonly ILogger logger;

		internal FileSystem(AppConfig appConfig, ILogger logger)
		{
			this.appConfig = appConfig;
			this.logger = logger;
		}

		internal bool TrySaveFile(HttpContent content, out Uri uri)
		{
			uri = new Uri(Path.Combine(appConfig.TemporaryDirectory, $"ServerExam{appConfig.ConfigurationFileExtension}"));

			try
			{
				var task = Task.Run(async () =>
				{
					return await content.ReadAsStreamAsync();
				});

				using (var data = task.GetAwaiter().GetResult())
				using (var file = new FileStream(uri.LocalPath, FileMode.Create))
				{
					data.Seek(0, SeekOrigin.Begin);
					data.CopyTo(file);
					data.Flush();
					file.Flush();
				}

				return true;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to save file '{uri.LocalPath}'!", e);
			}

			return false;
		}
	}
}
