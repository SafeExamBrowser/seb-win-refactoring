/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour.Operations;

namespace SafeExamBrowser.Core.Behaviour
{
	public class StartupController : IStartupController
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private IMessageBox messageBox;
		private INotificationInfo aboutInfo;
		private IProcessMonitor processMonitor;
		private ISettings settings;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private IText text;
		private IUiElementFactory uiFactory;
		private IWorkingArea workingArea;

		private IEnumerable<IOperation> startupOperations;

		public StartupController(
			IApplicationController browserController,
			IApplicationInfo browserInfo,
			ILogger logger,
			IMessageBox messageBox,
			INotificationInfo aboutInfo,
			IProcessMonitor processMonitor,
			ISettings settings,
			ITaskbar taskbar,
			IText text,
			IUiElementFactory uiFactory,
			IWorkingArea workingArea)
		{
			this.browserController = browserController;
			this.browserInfo = browserInfo;
			this.logger = logger;
			this.messageBox = messageBox;
			this.aboutInfo = aboutInfo;
			this.processMonitor = processMonitor;
			this.settings = settings;
			this.taskbar = taskbar;
			this.text = text;
			this.uiFactory = uiFactory;
			this.workingArea = workingArea;
		}

		public bool TryInitializeApplication(out Stack<IOperation> operations)
		{
			operations = new Stack<IOperation>();

			try
			{
				CreateStartupOperations();

				InitializeApplicationLog();
				InitializeSplashScreen();

				operations = PerformOperations();

				FinishInitialization();

				return true;
			}
			catch (Exception e)
			{
				LogAndShowException(e);
				RevertOperations(operations);
				FinishInitialization(false);

				return false;
			}
		}

		private Stack<IOperation> PerformOperations()
		{
			var operations = new Stack<IOperation>();

			foreach (var operation in startupOperations)
			{
				operations.Push(operation);

				operation.SplashScreen = splashScreen;
				operation.Perform();

				splashScreen.Progress();

				// TODO: Remove!
				Thread.Sleep(250);
			}

			return operations;
		}

		private void RevertOperations(Stack<IOperation> operations)
		{
			while (operations.Any())
			{
				var operation = operations.Pop();

				operation.Revert();
				splashScreen.Regress();

				// TODO: Remove!
				Thread.Sleep(250);
			}
		}

		private void CreateStartupOperations()
		{
			startupOperations = new IOperation[]
			{
				new ProcessMonitoringOperation(logger, processMonitor),
				new WorkingAreaOperation(logger, processMonitor, taskbar, workingArea),
				new TaskbarInitializationOperation(logger, aboutInfo, taskbar, uiFactory),
				new BrowserInitializationOperation(browserController, browserInfo, logger, taskbar, uiFactory)
			};
		}

		private void InitializeApplicationLog()
		{
			var titleLine = $"/* {settings.ProgramTitle}, Version {settings.ProgramVersion}{Environment.NewLine}";
			var copyrightLine = $"/* {settings.ProgramCopyright}{Environment.NewLine}";
			var emptyLine = $"/* {Environment.NewLine}";
			var githubLine = $"/* Please visit https://github.com/SafeExamBrowser for more information.";

			logger.Log($"{titleLine}{copyrightLine}{emptyLine}{githubLine}");
			logger.Log($"{Environment.NewLine}# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{Environment.NewLine}");
			logger.Info("--- Initiating startup procedure ---");
		}

		private void InitializeSplashScreen()
		{
			splashScreen = uiFactory.CreateSplashScreen(settings, text);
			splashScreen.SetMaxProgress(startupOperations.Count());
			splashScreen.UpdateText(Key.SplashScreen_StartupProcedure);
			splashScreen.InvokeShow();
		}

		private void LogAndShowException(Exception e)
		{
			logger.Error($"Failed to initialize application!", e);
			messageBox.Show(text.Get(Key.MessageBox_StartupError), text.Get(Key.MessageBox_StartupErrorTitle), icon: MessageBoxIcon.Error);
			logger.Info("Reverting operations...");
		}

		private void FinishInitialization(bool success = true)
		{
			if (success)
			{
				logger.Info("--- Application successfully initialized! ---");
				splashScreen.InvokeClose();
			}
			else
			{
				logger.Log($"{Environment.NewLine}# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			}
		}
	}
}
