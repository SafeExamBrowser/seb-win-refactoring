/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Text;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public partial class BinaryFormat
	{
		private LoadStatus ParsePlainData(Stream data, out Settings settings)
		{
			if (compressor.IsCompressed(data))
			{
				data = compressor.Decompress(data);
			}

			var buffer = new byte[4096];
			var bytesRead = 0;
			var xmlBuilder = new StringBuilder();

			data.Seek(0, SeekOrigin.Begin);

			do
			{
				bytesRead = data.Read(buffer, 0, buffer.Length);
				xmlBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
			} while (bytesRead > 0);

			var xml = xmlBuilder.ToString();

			// TODO: Parse XML data...

			settings = new Settings();
			settings.Browser.AllowAddressBar = true;
			settings.Browser.AllowConfigurationDownloads = true;

			return LoadStatus.Success;
		}
	}
}
