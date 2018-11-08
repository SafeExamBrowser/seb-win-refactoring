/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.ResourceLoaders
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
			if (resource.IsFile && File.Exists(resource.AbsolutePath))
			{
				logger.Debug($"Can load '{resource}' as it references an existing file.");

				return true;
			}

			logger.Debug($"Can't load '{resource}' since it isn't a file URI or no file exists at the specified path.");

			return false;
		}

		public byte[] Load(Uri resource)
		{
			logger.Debug($"Loading data from '{resource}'...");

			return File.ReadAllBytes(resource.AbsolutePath);
		}
	}
}
