/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Text;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class BinaryFormat : IDataFormat
	{
		private const int PREFIX_LENGTH = 4;

		private IDataCompressor compressor;
		private ILogger logger;

		public BinaryFormat(IDataCompressor compressor, ILogger logger)
		{
			this.compressor = compressor;
			this.logger = logger;
		}

		public bool CanParse(Stream data)
		{
			try
			{
				var longEnough = data.Length > PREFIX_LENGTH;

				if (longEnough)
				{
					var prefix = ParsePrefix(data);
					var success = TryDetermineFormat(prefix, out DataFormat format);

					logger.Debug($"'{data}' starting with '{prefix}' does {(success ? string.Empty : "not ")}match the binary format.");

					return success;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the binary format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data.Length / 1000.0} KB data matches the binary format!", e);
			}

			return false;
		}

		public LoadStatus TryParse(Stream data, out Settings settings, string adminPassword = null, string settingsPassword = null)
		{
			settings = new Settings();
			settings.Browser.AllowAddressBar = true;
			settings.Browser.StartUrl = "www.duckduckgo.com";
			settings.Browser.AllowConfigurationDownloads = true;

			return LoadStatus.Success;
		}

		private string ParsePrefix(Stream data)
		{
			var prefixData = new byte[PREFIX_LENGTH];

			if (compressor.IsCompressed(data))
			{
				prefixData = compressor.Peek(data, PREFIX_LENGTH);
			}
			else
			{
				data.Seek(0, SeekOrigin.Begin);
				data.Read(prefixData, 0, PREFIX_LENGTH);
			}

			return Encoding.UTF8.GetString(prefixData);
		}

		private bool TryDetermineFormat(string prefix, out DataFormat format)
		{
			format = default(DataFormat);

			switch (prefix)
			{
				case "pswd":
					format = DataFormat.Password;
					return true;
				case "pwcc":
					format = DataFormat.PasswordForConfigureClient;
					return true;
				case "plnd":
					format = DataFormat.PlainData;
					return true;
				case "pkhs":
					format = DataFormat.PublicKeyHash;
					return true;
				case "phsk":
					format = DataFormat.PublicKeyHashWithSymmetricKey;
					return true;
			}

			return false;
		}

		private enum DataFormat
		{
			Password = 1,
			PasswordForConfigureClient,
			PlainData,
			PublicKeyHash,
			PublicKeyHashWithSymmetricKey
		}
	}
}
