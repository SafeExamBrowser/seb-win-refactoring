/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Service;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Service.Communication;

namespace SafeExamBrowser.Service
{
	internal class CompositionRoot
	{
		private ILogger logger;

		internal IServiceController ServiceController { get; private set; }

		internal void BuildObjectGraph()
		{
			const string SERVICE_ADDRESS = "net.pipe://localhost/safeexambrowser/service";
			const int FIVE_SECONDS = 5000;

			InitializeLogging();

			var serviceHost = new ServiceHost(SERVICE_ADDRESS, new HostObjectFactory(), new ModuleLogger(logger, nameof(ServiceHost)), FIVE_SECONDS);
			var sessionContext = new SessionContext();

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IRepeatableOperation>();

			// TODO: bootstrapOperations.Enqueue(new RestoreOperation());
			bootstrapOperations.Enqueue(new CommunicationHostOperation(serviceHost, logger));

			// sessionOperations.Enqueue(new RuntimeConnectionOperation());
			// sessionOperations.Enqueue(new LogOperation());
			// sessionOperations.Enqueue(new RegistryOperation());
			// sessionOperations.Enqueue(new WindowsUpdateOperation());
			// sessionOperations.Enqueue(new SessionActivationOperation());

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new RepeatableOperationSequence(logger, sessionOperations);

			ServiceController = new ServiceController(bootstrapSequence, sessionSequence, serviceHost, sessionContext);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# Service started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log(string.Empty);
			logger?.Log($"# Service terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void InitializeLogging()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SafeExamBrowser));
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = DateTime.Now.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");
			var logFilePath = Path.Combine(logFolder, $"{logFilePrefix}_Service.log");
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

			logger = new Logger();
			logger.LogLevel = LogLevel.Debug;
			logger.Subscribe(logFileWriter);
			logFileWriter.Initialize();
		}
	}
}
