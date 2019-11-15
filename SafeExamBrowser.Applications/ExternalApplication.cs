/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplication : IApplication
	{
		private string executablePath;
		private IModuleLogger logger;
		private IList<IApplicationInstance> instances;
		private IProcessFactory processFactory;

		public ApplicationInfo Info { get; }

		public event InstanceStartedEventHandler InstanceStarted;

		internal ExternalApplication(string executablePath, ApplicationInfo info, IModuleLogger logger, IProcessFactory processFactory)
		{
			this.executablePath = executablePath;
			this.Info = info;
			this.logger = logger;
			this.instances = new List<IApplicationInstance>();
			this.processFactory = processFactory;
		}

		public void Initialize()
		{
			// Nothing to do here for now.
		}

		public void Start()
		{
			logger.Info("Starting application...");

			// TODO: Ensure that SEB does not crash if an application cannot be started!!

			var process = processFactory.StartNew(executablePath);
			var id = new ApplicationInstanceIdentifier(process.Id);
			var instance = new ExternalApplicationInstance(Info.Icon, id, logger.CloneFor($"{Info.Name} {id}"), process);

			instance.Initialize();
			instances.Add(instance);
			InstanceStarted?.Invoke(instance);
		}

		public void Terminate()
		{
			if (instances.Any())
			{
				logger.Info("Terminating application...");

				foreach (var instance in instances)
				{
					instance.Terminate();
				}
			}
		}
	}
}
