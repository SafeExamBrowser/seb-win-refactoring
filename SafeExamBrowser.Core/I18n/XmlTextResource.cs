/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using SafeExamBrowser.Contracts.I18n;

namespace SafeExamBrowser.Core.I18n
{
	public class XmlTextResource : ITextResource
	{
		public IDictionary<TextKey, string> LoadText()
		{
			var assembly = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = Path.GetDirectoryName(assembly) + $@"\{nameof(I18n)}\Text.xml";
			var xml = XDocument.Load(path);
			var text = new Dictionary<TextKey, string>();

			foreach (var definition in xml.Root.Descendants())
			{
				if (Enum.TryParse(definition.Name.LocalName, out TextKey key))
				{
					text[key] = definition.Value;
				}
			}

			return text;
		}
	}
}
