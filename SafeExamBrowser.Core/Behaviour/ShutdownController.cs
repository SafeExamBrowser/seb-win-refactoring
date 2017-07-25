/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class ShutdownController : IShutdownController
	{
		private ILogger logger;
		private ISettings settings;
		private ISplashScreen splashScreen;
		private IText text;
		private IUiElementFactory uiFactory;

		public ShutdownController(ILogger logger, ISettings settings, IText text, IUiElementFactory uiFactory)
		{
			this.logger = logger;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void FinalizeApplication(Queue<IOperation> operations)
		{
			try
			{
				InitializeSplashScreen();
				RevertOperations(operations);
				FinishFinalization();
			}
			catch (Exception e)
			{
				LogAndShowException(e);
				FinishFinalization(false);
			}
		}

		private void RevertOperations(Queue<IOperation> operations)
		{
			foreach (var operation in operations)
			{
				operation.SplashScreen = splashScreen;
				operation.Revert();

				// TODO: Remove!
				Thread.Sleep(250);
			}
		}

		private void InitializeSplashScreen()
		{
			splashScreen = uiFactory.CreateSplashScreen(settings, text);
			splashScreen.SetIndeterminate();
			splashScreen.UpdateText(Key.SplashScreen_ShutdownProcedure);
			splashScreen.InvokeShow();
			logger.Info("--- Initiating shutdown procedure ---");
		}

		private void LogAndShowException(Exception e)
		{
			logger.Error($"Failed to finalize application!", e);
			uiFactory.Show(text.Get(Key.MessageBox_ShutdownError), text.Get(Key.MessageBox_ShutdownErrorTitle), icon: MessageBoxIcon.Error);
		}

		private void FinishFinalization(bool success = true)
		{
			if (success)
			{
				logger.Info("--- Application successfully finalized! ---");
			}

			logger.Log($"{Environment.NewLine}# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			splashScreen?.InvokeClose();
		}
	}
}
