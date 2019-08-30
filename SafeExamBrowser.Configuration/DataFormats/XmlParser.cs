/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class XmlParser : IDataParser
	{
		private const string XML_PREFIX = "<?xm";

		private ILogger logger;

		public XmlParser(ILogger logger)
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
					var isXml = prefix == XML_PREFIX;

					logger.Debug($"'{data}' starting with '{prefix}' {(isXml ? "matches" : "does not match")} the {FormatType.Xml} format.");

					return isXml;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the {FormatType.Xml} format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data?.Length / 1000.0} KB data matches the {FormatType.Xml} format!", e);
			}

			return false;
		}

		public ParseResult TryParse(Stream data, PasswordParameters password = null)
		{
			var result = new ParseResult { Format = FormatType.Xml, Status = LoadStatus.InvalidData };
			var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

			data.Seek(0, SeekOrigin.Begin);

			using (var reader = XmlReader.Create(data, settings))
			{
				var hasRoot = reader.ReadToFollowing(XmlElement.Root);
				var hasDictionary = reader.ReadToDescendant(XmlElement.Dictionary);

				if (hasRoot && hasDictionary)
				{
					logger.Debug($"Found root node, starting to parse data...");

					result.Status = ParseDictionary(reader, out var rawData);
					result.RawData = rawData;

					logger.Debug($"Finished parsing -> Result: {result.Status}.");
				}
				else
				{
					logger.Error($"Could not find root {(!hasRoot ? $"node '{XmlElement.Root}'" : $"dictionary '{XmlElement.Dictionary}'")}!");
				}
			}

			return result;
		}

		private LoadStatus ParseArray(XmlReader reader, out List<object> array)
		{
			array = new List<object>();

			if (reader.IsEmptyElement)
			{
				reader.Read();
				reader.MoveToContent();

				return LoadStatus.Success;
			}

			reader.Read();
			reader.MoveToContent();

			while (reader.NodeType == XmlNodeType.Element)
			{
				var status = ParseElement(reader, out object element);

				if (status == LoadStatus.Success)
				{
					array.Add(element);
				}
				else
				{
					return status;
				}

				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlElement.Array)
			{
				reader.Read();
				reader.MoveToContent();

				return LoadStatus.Success;
			}

			logger.Error($"Expected closing tag for '{XmlElement.Array}', but found '{reader.Name}{reader.Value}'!");

			return LoadStatus.InvalidData;
		}

		private LoadStatus ParseDictionary(XmlReader reader, out Dictionary<string, object> dictionary)
		{
			dictionary = new Dictionary<string, object>();

			if (reader.IsEmptyElement)
			{
				reader.Read();
				reader.MoveToContent();

				return LoadStatus.Success;
			}

			reader.Read();
			reader.MoveToContent();

			while (reader.NodeType == XmlNodeType.Element)
			{
				var status = ParseKeyValuePair(reader, out var pair);

				if (status == LoadStatus.Success)
				{
					dictionary[pair.Key] = pair.Value;
				}
				else
				{
					return status;
				}

				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlElement.Dictionary)
			{
				reader.Read();
				reader.MoveToContent();

				return LoadStatus.Success;
			}

			logger.Error($"Expected closing tag for '{XmlElement.Dictionary}', but found '{reader.Name}{reader.Value}'!");

			return LoadStatus.InvalidData;
		}

		private LoadStatus ParseKeyValuePair(XmlReader reader, out KeyValuePair<string, object> pair)
		{
			var status = LoadStatus.InvalidData;
			var key = XNode.ReadFrom(reader) as XElement;

			pair = default(KeyValuePair<string, object>);

			if (key.Name.LocalName == XmlElement.Key)
			{
				reader.MoveToContent();
				status = ParseElement(reader, out object value);

				if (status == LoadStatus.Success)
				{
					pair = new KeyValuePair<string, object>(key.Value, value);
				}
			}
			else
			{
				logger.Error($"Expected element '{XmlElement.Key}', but found '{key}'!");
			}

			return status;
		}

		private LoadStatus ParseElement(XmlReader reader, out object element)
		{
			var array = default(List<object>);
			var dictionary = default(Dictionary<string, object>);
			var status = default(LoadStatus);
			var value = default(object);

			if (reader.Name == XmlElement.Array)
			{
				status = ParseArray(reader, out array);
			}
			else if (reader.Name == XmlElement.Dictionary)
			{
				status = ParseDictionary(reader, out dictionary);
			}
			else
			{
				status = ParseSimpleType(XNode.ReadFrom(reader) as XElement, out value);
			}

			element = array ?? dictionary ?? value;

			return status;
		}

		private LoadStatus ParseSimpleType(XElement element, out object value)
		{
			var status = LoadStatus.Success;

			value = null;

			switch (element.Name.LocalName)
			{
				case XmlElement.Data:
					value = Convert.FromBase64String(element.Value);
					break;
				case XmlElement.Date:
					value = XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.Utc);
					break;
				case XmlElement.False:
					value = false;
					break;
				case XmlElement.Integer:
					value = Convert.ToInt32(element.Value);
					break;
				case XmlElement.Real:
					value = Convert.ToDouble(element.Value);
					break;
				case XmlElement.String:
					value = element.IsEmpty ? null : element.Value;
					break;
				case XmlElement.True:
					value = true;
					break;
				default:
					status = LoadStatus.InvalidData;
					break;
			}

			if (status != LoadStatus.Success)
			{
				logger.Error($"Element '{element}' is not a supported value type!");
			}

			return status;
		}
	}
}
