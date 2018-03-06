/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Globalization;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.I18n;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	/// <summary>
	/// An operation to handle the initialization of an <see cref="IText"/> module with text data from the default directory.
	/// </summary>
	public class I18nOperation : IOperation
	{
		private ILogger logger;
		private IText text;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public I18nOperation(ILogger logger, IText text)
		{
			this.logger = logger;
			this.text = text;
		}

		public OperationResult Perform()
		{
			logger.Info($"Loading default text data (the currently active culture is '{CultureInfo.CurrentCulture.Name}')...");

			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = Path.GetDirectoryName(location) + $@"\{nameof(I18n)}\Text.xml";
			var textResource = new XmlTextResource(path);

			text.Initialize(textResource);

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			// Nothing to do here...
		}
	}
}
