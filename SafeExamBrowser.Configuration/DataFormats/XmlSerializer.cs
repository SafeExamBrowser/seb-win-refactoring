/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class XmlSerializer : IDataSerializer
	{
		private readonly ILogger logger;

		public XmlSerializer(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanSerialize(FormatType format)
		{
			return format == FormatType.Xml;
		}

		public SerializeResult TrySerialize(IDictionary<string, object> data, EncryptionParameters encryption = null)
		{
			var result = new SerializeResult();
			var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(), Indent = true };
			var stream = new MemoryStream();

			logger.Debug("Starting to serialize data...");

			using (var writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteDocType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "https://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
				writer.WriteStartElement(XmlElement.Root);
				writer.WriteAttributeString("version", "1.0");

				result.Status = WriteDictionary(writer, data);
				result.Data = stream;

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();
			}

			logger.Debug($"Finished serialization -> Result: {result.Status}.");

			return result;
		}

		private SaveStatus WriteArray(XmlWriter writer, List<object> array)
		{
			var status = SaveStatus.Success;

			writer.WriteStartElement(XmlElement.Array);

			foreach (var item in array)
			{
				status = WriteElement(writer, item);

				if (status != SaveStatus.Success)
				{
					break;
				}
			}

			writer.WriteEndElement();

			return status;
		}

		private SaveStatus WriteDictionary(XmlWriter writer, IDictionary<string, object> data)
		{
			var status = SaveStatus.Success;

			writer.WriteStartElement(XmlElement.Dictionary);

			foreach (var item in data.OrderBy(i => i.Key))
			{
				status = WriteKeyValuePair(writer, item);

				if (status != SaveStatus.Success)
				{
					break;
				}
			}

			writer.WriteEndElement();

			return status;
		}

		private SaveStatus WriteKeyValuePair(XmlWriter writer, KeyValuePair<string, object> item)
		{
			var status = SaveStatus.InvalidData;

			if (item.Key != null)
			{
				writer.WriteElementString(XmlElement.Key, item.Key);
				status = WriteElement(writer, item.Value);
			}
			else
			{
				logger.Error($"Key of item '{item}' is null!");
			}

			return status;
		}

		private SaveStatus WriteElement(XmlWriter writer, object element)
		{
			var status = default(SaveStatus);

			if (element is List<object> array)
			{
				status = WriteArray(writer, array);
			}
			else if (element is Dictionary<string, object> dictionary)
			{
				status = WriteDictionary(writer, dictionary);
			}
			else
			{
				status = WriteSimpleType(writer, element);
			}

			return status;
		}

		private SaveStatus WriteSimpleType(XmlWriter writer, object element)
		{
			var name = default(string);
			var value = default(string);
			var status = SaveStatus.Success;

			switch (element)
			{
				case byte[] data:
					name = XmlElement.Data;
					value = Convert.ToBase64String(data);
					break;
				case DateTime date:
					name = XmlElement.Date;
					value = XmlConvert.ToString(date, XmlDateTimeSerializationMode.Utc);
					break;
				case bool boolean when boolean == false:
					name = XmlElement.False;
					break;
				case bool boolean when boolean == true:
					name = XmlElement.True;
					break;
				case int integer:
					name = XmlElement.Integer;
					value = integer.ToString(NumberFormatInfo.InvariantInfo);
					break;
				case double real:
					name = XmlElement.Real;
					value = real.ToString(NumberFormatInfo.InvariantInfo);
					break;
				case string text:
					name = XmlElement.String;
					value = text;
					break;
				case null:
					name = XmlElement.String;
					break;
				default:
					status = SaveStatus.InvalidData;
					break;
			}

			if (status == SaveStatus.Success)
			{
				writer.WriteElementString(name, value);
			}
			else
			{
				logger.Error($"Element '{element}' is not supported!");
			}

			return status;
		}
	}
}
