/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.I18n
{
	/// <summary>
	/// Default implementation of an <see cref="ITextResource"/> for text data in XML.
	/// </summary>
	public class XmlTextResource : ITextResource
	{
		private Stream data;

		/// <summary>
		/// Initializes a new text resource for an XML data stream.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the given stream is null.</exception>
		public XmlTextResource(Stream data)
		{
			this.data = data ?? throw new ArgumentNullException(nameof(data));
		}

		public IDictionary<TextKey, string> LoadText()
		{
			var xml = XDocument.Load(data);
			var text = new Dictionary<TextKey, string>();

			foreach (var definition in xml.Root.Descendants())
			{
				var isEntry = definition.Name.LocalName == "Entry";
				var hasValidKey = Enum.TryParse(definition.Attribute("key")?.Value, out TextKey key);

				if (isEntry && hasValidKey)
				{
					text[key] = definition.Value?.Trim() ?? string.Empty;
				}
			}

			return text;
		}
	}
}
