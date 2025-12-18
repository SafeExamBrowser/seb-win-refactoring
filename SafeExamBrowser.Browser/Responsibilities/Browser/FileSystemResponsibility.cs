/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class FileSystemResponsibility : BrowserResponsibility
	{
		public FileSystemResponsibility(BrowserApplicationContext context) : base(context)
		{
		}

		public override void Assume(BrowserTask task)
		{
			switch (task)
			{
				case BrowserTask.InitializeFileSystem:
					InitializeDownAndUploadDirectory();
					break;
				case BrowserTask.FinalizeFileSystem:
					FinalizeDownAndUploadDirectory();
					break;
			}
		}

		private void FinalizeDownAndUploadDirectory()
		{
			if (Settings.UseTemporaryDownAndUploadDirectory)
			{
				try
				{
					Directory.Delete(Settings.DownAndUploadDirectory, true);
					Logger.Info("Deleted temporary down- and upload directory.");
				}
				catch (Exception e)
				{
					Logger.Error("Failed to delete temporary down- and upload directory!", e);
				}
			}
		}

		private void InitializeDownAndUploadDirectory()
		{
			if (Settings.UseTemporaryDownAndUploadDirectory)
			{
				InitializeTemporaryDownAndUploadDirectory();
			}
			else if (!string.IsNullOrEmpty(Settings.DownAndUploadDirectory))
			{
				InitializeCustomDownAndUploadDirectory();
			}
		}

		private void InitializeCustomDownAndUploadDirectory()
		{
			if (!Directory.Exists(Environment.ExpandEnvironmentVariables(Settings.DownAndUploadDirectory)))
			{
				Logger.Warn("The configured down- and upload directory does not exist! Falling back to the default directory...");
				Settings.DownAndUploadDirectory = default;
			}
			else
			{
				Logger.Debug("Using custom down- and upload directory as defined in the active configuration.");
			}
		}

		private void InitializeTemporaryDownAndUploadDirectory()
		{
			try
			{
				Settings.DownAndUploadDirectory = Path.Combine(AppConfig.TemporaryDirectory, Path.GetRandomFileName());
				Directory.CreateDirectory(Settings.DownAndUploadDirectory);
				Logger.Info($"Created temporary down- and upload directory.");
			}
			catch (Exception e)
			{
				Logger.Error("Failed to create temporary down- and upload directory!", e);
			}
		}
	}
}
