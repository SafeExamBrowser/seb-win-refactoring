/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
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
	/// Default implementation of <see cref="ITextResource"/> to load text data from XML files.
	/// </summary>
	public class XmlTextResource : ITextResource
	{
		private string path;

		/// <summary>
		/// Initializes a new text resource for an XML file located at the specified path.
		/// </summary>
		/// <exception cref="ArgumentException">If the specifed file does not exist.</exception>
		/// <exception cref="ArgumentNullException">If the given path is null.</exception>
		public XmlTextResource(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (!File.Exists(path))
			{
				throw new ArgumentException("The specified file does not exist!");
			}

			this.path = path;
		}

		public IDictionary<TextKey, string> LoadText()
		{
			var xml = XDocument.Load(path);
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
