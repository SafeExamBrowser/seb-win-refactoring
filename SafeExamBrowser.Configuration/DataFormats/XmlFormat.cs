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
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class XmlFormat : IDataFormat
	{
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

		public LoadStatus TryParse(Stream data, Settings settings, string password = null, bool passwordIsHash = false)
		{
			var status = LoadStatus.InvalidData;
			var xmlSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

			data.Seek(0, SeekOrigin.Begin);

			using (var reader = XmlReader.Create(data, xmlSettings))
			{
				var hasRoot = reader.ReadToFollowing(ROOT_NODE);
				var hasDictionary = reader.ReadToDescendant(DataTypes.DICTIONARY);
				var rawData = new Dictionary<string, object>();

				if (hasRoot && hasDictionary)
				{
					logger.Debug($"Found root node, starting to parse data...");
					status = ParseDictionary(reader, rawData);

					if (status == LoadStatus.Success)
					{
						logger.Debug("Mapping raw settings data...");
						rawData.MapTo(settings);
					}

					logger.Debug($"Finished parsing -> Result: {status}.");
				}
				else
				{
					logger.Error($"Could not find root {(!hasRoot ? $"node '{ROOT_NODE}'" : $"dictionary '{DataTypes.DICTIONARY}'")}!");
				}
			}

			return status;
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

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == DataTypes.ARRAY)
			{
				return LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{DataTypes.ARRAY}', but found '{reader.Name}{reader.Value}'!");

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

			if (reader.NodeType == XmlNodeType.EndElement && reader.Name == DataTypes.DICTIONARY)
			{
				return LoadStatus.Success;
			}
			else
			{
				logger.Error($"Expected closing tag for '{DataTypes.DICTIONARY}', but found '{reader.Name}{reader.Value}'!");

				return LoadStatus.InvalidData;
			}
		}

		private LoadStatus ParseKeyValuePair(XmlReader reader, Dictionary<string, object> dictionary)
		{
			var key = XNode.ReadFrom(reader) as XElement;

			if (key.Name.LocalName != DataTypes.KEY)
			{
				logger.Error($"Expected element '{DataTypes.KEY}', but found '{key}'!");

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

			if (reader.Name == DataTypes.ARRAY)
			{
				array = new List<object>();
				status = ParseArray(reader, array);
			}
			else if (reader.Name == DataTypes.DICTIONARY)
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
			value = null;

			switch (element.Name.LocalName)
			{
				case DataTypes.DATA:
					value = Convert.FromBase64String(element.Value);
					break;
				case DataTypes.DATE:
					value = XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.Utc);
					break;
				case DataTypes.FALSE:
					value = false;
					break;
				case DataTypes.INTEGER:
					value = Convert.ToInt32(element.Value);
					break;
				case DataTypes.REAL:
					value = Convert.ToDouble(element.Value);
					break;
				case DataTypes.STRING:
					value = element.Value;
					break;
				case DataTypes.TRUE:
					value = true;
					break;
			}

			if (value == null)
			{
				logger.Error($"Element '{element}' is not supported!");

				return LoadStatus.InvalidData;
			}

			return LoadStatus.Success;
		}

		private struct DataTypes
		{
			public const string ARRAY = "array";
			public const string DATA = "data";
			public const string DATE = "date";
			public const string DICTIONARY = "dict";
			public const string FALSE = "false";
			public const string INTEGER = "integer";
			public const string KEY = "key";
			public const string REAL = "real";
			public const string STRING = "string";
			public const string TRUE = "true";
		}
	}
}
