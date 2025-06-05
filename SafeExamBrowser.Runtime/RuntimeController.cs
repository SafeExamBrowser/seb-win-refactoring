/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime
{
	internal class RuntimeController
	{
		private readonly ILogger logger;
		private readonly IOperationSequence bootstrapSequence;
		private readonly IResponsibilityCollection<RuntimeTask> responsibilities;
		private readonly RuntimeContext runtimeContext;
		private readonly IRuntimeWindow runtimeWindow;
		private readonly ISplashScreen splashScreen;

		private bool SessionIsRunning => runtimeContext.Current != default;

		internal RuntimeController(
			ILogger logger,
			IOperationSequence bootstrapSequence,
			IResponsibilityCollection<RuntimeTask> responsibilities,
			RuntimeContext runtimeContext,
			IRuntimeWindow runtimeWindow,
			ISplashScreen splashScreen)
		{
			this.bootstrapSequence = bootstrapSequence;
			this.responsibilities = responsibilities;
			this.logger = logger;
			this.runtimeWindow = runtimeWindow;
			this.runtimeContext = runtimeContext;
			this.splashScreen = splashScreen;
		}

		internal bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			// We need to show the runtime window here already, this way implicitly setting it as the runtime application's main window.
			// Otherwise, the splash screen is considered as the main window and thus the operating system and/or WPF does not correctly
			// activate the runtime window once bootstrapping has finished, which in turn leads to undesired user interface behavior.
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.SetIndeterminate();

			splashScreen.Show();
			splashScreen.BringToForeground();

			var initialized = bootstrapSequence.TryPerform() == OperationResult.Success;

			if (initialized)
			{
				responsibilities.Delegate(RuntimeTask.RegisterEvents);
				splashScreen.Hide();

				logger.Info("Application successfully initialized.");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);

				responsibilities.Delegate(RuntimeTask.StartSession);
			}
			else
			{
				logger.Info("Application startup aborted!");
				logger.Log(string.Empty);

				responsibilities.Delegate(RuntimeTask.ShowStartupError);
			}

			return initialized && SessionIsRunning;
		}

		internal void Terminate()
		{
			responsibilities.Delegate(RuntimeTask.DeregisterEvents);

			if (SessionIsRunning)
			{
				responsibilities.Delegate(RuntimeTask.StopSession);
			}

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow.Close();

			splashScreen.Show();
			splashScreen.BringToForeground();

			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			var success = bootstrapSequence.TryRevert() == OperationResult.Success;

			if (success)
			{
				logger.Info("Application successfully finalized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Shutdown procedure failed!");
				logger.Log(string.Empty);

				responsibilities.Delegate(RuntimeTask.ShowShutdownError);
			}

			splashScreen.Close();
		}
	}
}
