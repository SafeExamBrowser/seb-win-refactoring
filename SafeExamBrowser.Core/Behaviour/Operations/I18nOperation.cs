/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Globalization;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.I18n;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class I18nOperation : IOperation
	{
		private ILogger logger;
		private IText text;

		public ISplashScreen SplashScreen { private get; set; }

		public I18nOperation(ILogger logger, IText text)
		{
			this.logger = logger;
			this.text = text;
		}

		public void Perform()
		{
			logger.Info($"Loading default text data (the currently active culture is '{CultureInfo.CurrentCulture.Name}')...");

			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = Path.GetDirectoryName(location) + $@"\{nameof(I18n)}\Text.xml";
			var textResource = new XmlTextResource(path);

			text.Initialize(textResource);
		}

		public void Revert()
		{
			// Nothing to do here...
		}
	}
}
