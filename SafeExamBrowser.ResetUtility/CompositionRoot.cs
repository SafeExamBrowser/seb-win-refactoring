/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Windows;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Logging;
using SafeExamBrowser.ResetUtility.Procedure;
using SafeExamBrowser.ResetUtility.UserInterface;

namespace SafeExamBrowser.ResetUtility
{
	internal class CompositionRoot
	{
		private ILogger logger;

		internal Window MainWindow { get; private set; }
		internal ProcedureStep InitialStep { get; private set; }

		internal void BuildObjectGraph()
		{
			InitializeLogging();

			var context = new AppContext
			{
				Logger = logger,
				UserInterface = new MainWindow()
			};

			MainWindow = context.UserInterface as Window;
			InitialStep = new Initialization(context);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
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
