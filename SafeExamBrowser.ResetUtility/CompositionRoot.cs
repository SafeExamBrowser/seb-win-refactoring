/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using SafeExamBrowser.Lockdown;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.ResetUtility.Procedure;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.SystemComponents;

namespace SafeExamBrowser.ResetUtility
{
	internal class CompositionRoot
	{
		private ProcedureContext context;
		private ILogger logger;

		internal ProcedureStep InitialStep { get; private set; }
		internal NativeMethods NativeMethods { get; private set; }

		internal void BuildObjectGraph()
		{
			InitializeLogging();
			InitializeContext();

			InitialStep = new Initialization(context);
			NativeMethods = new NativeMethods();
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger.Log(string.Empty);
			logger?.Log($"# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private IList<MainMenuOption> BuildMainMenu(ProcedureContext context)
		{
			return new List<MainMenuOption>
			{
				new MainMenuOption { IsSelected = true, NextStep = new Restore(context), Result = ProcedureStepResult.Continue, Text = "Restore system configuration via backup mechanism" },
				new MainMenuOption { NextStep = new Reset(context), Result = ProcedureStepResult.Continue, Text = "Reset system configuration to default values" },
				new MainMenuOption { NextStep = new Procedure.Version(context), Result = ProcedureStepResult.Continue, Text = "Show version information" },
				new MainMenuOption { NextStep = new Log(context), Result = ProcedureStepResult.Continue, Text = "Show application log" },
				new MainMenuOption { Result = ProcedureStepResult.Terminate, Text = "Exit" }
			};
		}

		private IFeatureConfigurationBackup CreateBackup(string filePath)
		{
			return new FeatureConfigurationBackup(filePath, new ModuleLogger(logger, nameof(FeatureConfigurationBackup)));
		}

		private void InitializeContext()
		{
			context = new ProcedureContext();
			context.ConfigurationFactory = new FeatureConfigurationFactory(new ModuleLogger(logger, nameof(FeatureConfigurationFactory)));
			context.CreateBackup = CreateBackup;
			context.Logger = logger;
			context.MainMenu = BuildMainMenu(context);
			context.Update = new SystemConfigurationUpdate(new ModuleLogger(logger, nameof(SystemConfigurationUpdate)));
			context.UserInfo = new UserInfo(new ModuleLogger(logger, nameof(UserInfo)));
		}

		private void InitializeLogging()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SafeExamBrowser));
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = DateTime.Now.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");
			var logFilePath = Path.Combine(logFolder, $"{logFilePrefix}_{nameof(ResetUtility)}.log");
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

			logger = new Logger();
			logger.LogLevel = LogLevel.Debug;
			logger.Subscribe(logFileWriter);
			logFileWriter.Initialize();
		}
	}
}
