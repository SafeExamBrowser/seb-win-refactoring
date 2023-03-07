/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.DataResources;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataResources
{
	public class FileResourceLoader : IResourceLoader
	{
		private ILogger logger;

		public FileResourceLoader(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanLoad(Uri resource)
		{
			var exists = resource.IsFile && File.Exists(resource.LocalPath);

			if (exists)
			{
				logger.Debug($"Can load '{resource}' as it is an existing file.");
			}
			else
			{
				logger.Debug($"Can't load '{resource}' as it isn't an existing file.");
			}

			return exists;
		}

		public LoadStatus TryLoad(Uri resource, out Stream data)
		{
			logger.Debug($"Loading data from '{resource}'...");
			data = new FileStream(resource.LocalPath, FileMode.Open, FileAccess.Read);
			logger.Debug($"Created '{data}' for {data.Length / 1000.0} KB data in '{resource}'.");

			return LoadStatus.Success;
		}
	}
}
