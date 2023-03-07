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
	public class FileResourceSaver : IResourceSaver
	{
		private ILogger logger;

		public FileResourceSaver(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanSave(Uri destination)
		{
			var isFullPath = destination.IsFile && Path.IsPathRooted(destination.LocalPath);

			if (isFullPath)
			{
				logger.Debug($"Can save data as '{destination}' since it defines an absolute file path.");
			}
			else
			{
				logger.Debug($"Can't save data as '{destination}' since it doesn't define an absolute file path.");
			}

			return isFullPath;
		}

		public SaveStatus TrySave(Uri destination, Stream data)
		{
			var directory = Path.GetDirectoryName(destination.LocalPath);

			logger.Debug($"Attempting to save '{data}' with {data.Length / 1000.0} KB data as '{destination}'...");

			if (!Directory.Exists(directory))
			{
				logger.Debug($"Creating directory '{directory}'...");
				Directory.CreateDirectory(directory);
			}

			using (var fileStream = new FileStream(destination.LocalPath, FileMode.Create))
			{
				data.Seek(0, SeekOrigin.Begin);
				data.CopyTo(fileStream);
			}

			logger.Debug($"Successfully saved data as '{destination}'.");

			return SaveStatus.Success;
		}
	}
}
