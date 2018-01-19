/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class StartupController : IStartupController
	{
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private ISplashScreen splashScreen;
		private ISystemInfo systemInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		private Stack<IOperation> stack = new Stack<IOperation>();

		public StartupController(ILogger logger, IRuntimeInfo runtimeInfo, ISystemInfo systemInfo, IText text, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.runtimeInfo = runtimeInfo;
			this.systemInfo = systemInfo;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryInitializeApplication(Queue<IOperation> operations)
		{
			var success = false;

			try
			{
				Initialize(operations.Count);
				success = Perform(operations);

				if (!success)
				{
					RevertOperations();
				}

				Finish(success);
			}
			catch (Exception e)
			{
				LogAndShowException(e);
				Finish(false);
			}

			return success;
		}

		private bool Perform(Queue<IOperation> operations)
		{
			foreach (var operation in operations)
			{
				stack.Push(operation);
				operation.SplashScreen = splashScreen;

				try
				{
					operation.Perform();
				}
				catch (Exception e)
				{
					LogAndShowException(e);

					return false;
				}

				if (operation.AbortStartup)
				{
					return false;
				}

				splashScreen.Progress();
			}

			return true;
		}

		private void RevertOperations()
		{
			while (stack.Any())
			{
				var operation = stack.Pop();

				try
				{
					operation.Revert();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to revert operation '{operation.GetType().Name}'!", e);
				}

				splashScreen.Regress();
			}
		}

		private void Initialize(int operationCount)
		{
			logger.Info("--- Initiating startup procedure ---");

			splashScreen = uiFactory.CreateSplashScreen(runtimeInfo, text);
			splashScreen.SetMaxProgress(operationCount);
			splashScreen.UpdateText(TextKey.SplashScreen_StartupProcedure);
			splashScreen.InvokeShow();
		}

		private void LogAndShowException(Exception e)
		{
			logger.Error($"Failed to initialize application!", e);
			uiFactory.Show(text.Get(TextKey.MessageBox_StartupError), text.Get(TextKey.MessageBox_StartupErrorTitle), icon: MessageBoxIcon.Error);
			logger.Info("Reverting operations...");
		}

		private void Finish(bool success = true)
		{
			if (success)
			{
				logger.Info("--- Application successfully initialized! ---");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("--- Startup procedure aborted! ---");
			}

			splashScreen?.InvokeClose();
		}
	}
}
