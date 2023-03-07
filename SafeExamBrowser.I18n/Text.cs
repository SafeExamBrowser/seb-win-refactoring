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
using System.Reflection;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.I18n
{
	/// <summary>
	/// Default implementation of <see cref="IText"/>.
	/// </summary>
	public class Text : IText
	{
		private ILogger logger;
		private IDictionary<TextKey, string> text;

		public Text(ILogger logger)
		{
			this.logger = logger;
			this.text = new Dictionary<TextKey, string>();
		}

		public string Get(TextKey key)
		{
			return text.ContainsKey(key) ? text[key] : $"Could not find text for key '{key}'!";
		}

		public void Initialize()
		{
			try
			{
				var assembly = Assembly.GetAssembly(typeof(Text));
				var data = default(Stream);
				var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
				var path = $"{typeof(Text).Namespace}.Data";
				var resource = default(ITextResource);

				try
				{
					logger.Debug($"System language is '{language}', trying to load data...");
					data = assembly.GetManifestResourceStream($"{path}.{language}.xml");
				}
				catch (FileNotFoundException)
				{
				}

				if (data == default(Stream))
				{
					logger.Warn($"Could not find data for language '{language}'! Loading default language 'en'...");
					data = assembly.GetManifestResourceStream($"{path}.en.xml");
				}

				using (data)
				{
					resource = new XmlTextResource(data);
					text = resource.LoadText();
				}

				logger.Debug("Data successfully loaded.");
			}
			catch (Exception e)
			{
				logger.Error("Failed to initialize data!", e);
			}
		}
	}
}
