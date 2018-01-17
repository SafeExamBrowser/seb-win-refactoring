/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class ShutdownController : IShutdownController
	{
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private ISplashScreen splashScreen;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public ShutdownController(ILogger logger, IRuntimeInfo runtimeInfo, IText text, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.runtimeInfo = runtimeInfo;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void FinalizeApplication(Queue<IOperation> operations)
		{
			try
			{
				Initialize();
				Revert(operations);
				Finish();
			}
			catch (Exception e)
			{
				LogAndShowException(e);
				Finish(false);
			}
		}

		private void Revert(Queue<IOperation> operations)
		{
			foreach (var operation in operations)
			{
				operation.SplashScreen = splashScreen;

				try
				{
					operation.Revert();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to revert operation '{operation.GetType().Name}'!", e);
				}
			}
		}

		private void Initialize()
		{
			logger.Log(string.Empty);
			logger.Info("--- Initiating shutdown procedure ---");

			splashScreen = uiFactory.CreateSplashScreen(runtimeInfo, text);
			splashScreen.SetIndeterminate();
			splashScreen.UpdateText(TextKey.SplashScreen_ShutdownProcedure);
			splashScreen.InvokeShow();
		}

		private void LogAndShowException(Exception e)
		{
			logger.Error($"Failed to finalize application!", e);
			uiFactory.Show(text.Get(TextKey.MessageBox_ShutdownError), text.Get(TextKey.MessageBox_ShutdownErrorTitle), icon: MessageBoxIcon.Error);
		}

		private void Finish(bool success = true)
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
