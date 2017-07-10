/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Configuration
{
	public class ShutdownController : IShutdownController
	{
		private ILogger logger;
		private IMessageBox messageBox;
		private IText text;

		public ShutdownController(ILogger logger, IMessageBox messageBox, IText text)
		{
			this.logger = logger;
			this.messageBox = messageBox;
			this.text = text;
		}

		public void FinalizeApplication()
		{
			try
			{
				// TODO:
				// - Gather TODOs!

				logger.Log($"{Environment.NewLine}# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to finalize application!", e);
				messageBox.Show(text.Get(Key.MessageBox_ShutdownError), text.Get(Key.MessageBox_ShutdownErrorTitle), icon: MessageBoxIcon.Error);
			}
		}
	}
}
