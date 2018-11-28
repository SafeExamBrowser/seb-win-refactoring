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
using System.Xml;
using System.Xml.Linq;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class XmlFormat : IDataFormat
	{
		private const string ARRAY = "array";
		private const string DICTIONARY = "dict";
		private const string KEY = "key";
		private const string ROOT_NODE = "plist";
		private const string XML_PREFIX = "<?xm";

		private ILogger logger;

		public XmlFormat(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanParse(Stream data)
		{
			try
			{
				var prefixData = new byte[XML_PREFIX.Length];
				var longEnough = data.Length > prefixData.Length;

				if (longEnough)
				{
					data.Seek(0, SeekOrigin.Begin);
					data.Read(prefixData, 0, prefixData.Length);
					var prefix = Encoding.UTF8.GetString(prefixData);
					var success = prefix == XML_PREFIX;

					logger.Debug($"'{data}' starting with '{prefix}' does {(success ? string.Empty : "not ")}match the XML format.");

					return success;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the XML format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data.Length / 1000.0} KB data matches the XML format!", e);
			}

			return false;
		}

		public LoadStatus TryParse(Stream data, out Settings settings, string password = null, bool passwordIsHash = false)
		{
			var status = LoadStatus.InvalidData;

			settings = new Settings();
			data.Seek(0, SeekOrigin.Begin);

			using (var reader = XmlReader.Create(data, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
			{
				var hasRoot = reader.ReadToFollowing(ROOT_NODE);

				if (hasRoot)
				{
					logger.Debug($"Found root node, starting to parse data...");
					Parse(reader, settings, out status);
					logger.Debug($"Finished parsing -> Result: {status}.");
				}
				else
				{
					logger.Error($"Could not find root node '{ROOT_NODE}'!");
				}
			}

			return status;
		}

		private void Parse(XmlReader reader, Settings settings, out LoadStatus status)
		{
			status = LoadStatus.Success;

			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.Name)
				{
					case ARRAY:
						ParseArray(reader, settings, out status);
						break;
					case DICTIONARY:
						ParseDictionary(reader, settings, out status);
						break;
					case KEY:
						ParseKeyValuePair(reader, settings, out status);
						break;
					case ROOT_NODE:
						break;
					default:
						status = LoadStatus.InvalidData;
						logger.Error($"Detected invalid element '{reader.Name}'!");
						break;
				}
			}

			reader.Read();
			reader.MoveToContent();

			if (!reader.EOF && status == LoadStatus.Success)
			{
				Parse(reader, settings, out status);
			}
		}

		private void ParseArray(XmlReader reader, Settings settings, out LoadStatus status)
		{
			reader.Read();
			reader.MoveToContent();

			while (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.Name == ARRAY)
				{
					ParseArray(reader, settings, out status);
				}
				else if (reader.Name == DICTIONARY)
				{
					ParseDictionary(reader, settings, out status);
				}
				else
				{
					var item = XNode.ReadFrom(reader) as XElement;

					// TODO: Map data...
				}

				reader.Read();
				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == ARRAY)
			{
				status = LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{ARRAY}', but found '{reader.Name}{reader.Value}'!");
				status = LoadStatus.InvalidData;
			}
		}

		private void ParseDictionary(XmlReader reader, Settings settings, out LoadStatus status)
		{
			reader.Read();
			reader.MoveToContent();

			while (reader.NodeType == XmlNodeType.Element)
			{
				ParseKeyValuePair(reader, settings, out status);

				reader.Read();
				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == DICTIONARY)
			{
				status = LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{DICTIONARY}', but found '{reader.Name}{reader.Value}'!");
				status = LoadStatus.InvalidData;
			}
		}

		private void ParseKeyValuePair(XmlReader reader, Settings settings, out LoadStatus status)
		{
			var key = XNode.ReadFrom(reader) as XElement;

			reader.Read();
			reader.MoveToContent();

			if (reader.Name == ARRAY)
			{
				ParseArray(reader, settings, out status);
			}
			else if (reader.Name == DICTIONARY)
			{
				ParseDictionary(reader, settings, out status);
			}
			else
			{
				var value = XNode.ReadFrom(reader) as XElement;

				// TODO: Map data...

				status = LoadStatus.Success;
			}
		}
	}
}
