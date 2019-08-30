/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Globalization;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.Operations
{
	/// <summary>
	/// An operation to handle the initialization of an <see cref="IText"/> module with text data from the default directory.
	/// </summary>
	public class I18nOperation : IOperation
	{
		private ILogger logger;
		private IText text;
		private ITextResource textResource;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public I18nOperation(ILogger logger, IText text, ITextResource textResource)
		{
			this.logger = logger;
			this.text = text;
			this.textResource = textResource;
		}

		public OperationResult Perform()
		{
			logger.Info($"Loading default text data (the currently active culture is '{CultureInfo.CurrentCulture.Name}')...");

			text.Initialize(textResource);

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
