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
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Service;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Service.Communication;
using SafeExamBrowser.Service.Operations;

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

			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), new ModuleLogger(logger, nameof(ProxyFactory)));
			var serviceHost = new ServiceHost(SERVICE_ADDRESS, new HostObjectFactory(), new ModuleLogger(logger, nameof(ServiceHost)), FIVE_SECONDS);
			var sessionContext = new SessionContext();

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IOperation>();

			// TODO: bootstrapOperations.Enqueue(new RestoreOperation());
			bootstrapOperations.Enqueue(new CommunicationHostOperation(serviceHost, logger));
			bootstrapOperations.Enqueue(new ServiceEventCleanupOperation(logger, sessionContext));

			sessionOperations.Enqueue(new SessionInitializationOperation(logger, CreateLogWriter, serviceHost, sessionContext));
			// TODO: sessionOperations.Enqueue(new RegistryOperation());
			//       sessionOperations.Enqueue(new WindowsUpdateOperation());
			sessionOperations.Enqueue(new SessionActivationOperation(logger, sessionContext));

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new OperationSequence(logger, sessionOperations);

			ServiceController = new ServiceController(logger, bootstrapSequence, sessionSequence, serviceHost, sessionContext);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# Service started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
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

		private ILogObserver CreateLogWriter(string filePath)
		{
			var writer = new LogFileWriter(new DefaultLogFormatter(), filePath);

			writer.Initialize();

			return writer;
		}
	}
}
