/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Logging;

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
					var success = prefix == XML_PREFIX;

					logger.Debug($"'{data}' starting with '{prefix}' does {(success ? string.Empty : "not ")}match the {FormatType.Xml} format.");

					return success;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the {FormatType.Xml} format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data.Length / 1000.0} KB data matches the {FormatType.Xml} format!", e);
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
				var rawData = new Dictionary<string, object>();

				if (hasRoot && hasDictionary)
				{
					logger.Debug($"Found root node, starting to parse data...");

					result.Status = ParseDictionary(reader, rawData);
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

		private LoadStatus ParseArray(XmlReader reader, List<object> array)
		{
			if (reader.IsEmptyElement)
			{
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

				reader.Read();
				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlElement.Array)
			{
				return LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{XmlElement.Array}', but found '{reader.Name}{reader.Value}'!");

				return LoadStatus.InvalidData;
			}
		}

		private LoadStatus ParseDictionary(XmlReader reader, Dictionary<string, object> dictionary)
		{
			if (reader.IsEmptyElement)
			{
				return LoadStatus.Success;
			}

			reader.Read();
			reader.MoveToContent();

			while (reader.NodeType == XmlNodeType.Element)
			{
				var status = ParseKeyValuePair(reader, dictionary);

				if (status != LoadStatus.Success)
				{
					return status;
				}

				reader.Read();
				reader.MoveToContent();
			}

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlElement.Dictionary)
			{
				return LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{XmlElement.Dictionary}', but found '{reader.Name}{reader.Value}'!");

				return LoadStatus.InvalidData;
			}
		}

		private LoadStatus ParseKeyValuePair(XmlReader reader, Dictionary<string, object> dictionary)
		{
			var key = XNode.ReadFrom(reader) as XElement;

			if (key.Name.LocalName != XmlElement.Key)
			{
				logger.Error($"Expected element '{XmlElement.Key}', but found '{key}'!");

				return LoadStatus.InvalidData;
			}

			reader.Read();
			reader.MoveToContent();

			var status = ParseElement(reader, out object value);

			if (status == LoadStatus.Success)
			{
				dictionary[key.Value] = value;
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
				array = new List<object>();
				status = ParseArray(reader, array);
			}
			else if (reader.Name == XmlElement.Dictionary)
			{
				dictionary = new Dictionary<string, object>();
				status = ParseDictionary(reader, dictionary);
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
				logger.Error($"Element '{element}' is not supported!");
			}

			return status;
		}
	}
}
