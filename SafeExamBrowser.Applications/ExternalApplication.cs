/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
		private IList<ExternalApplicationInstance> instances;
		private IProcessFactory processFactory;

		public event WindowsChangedEventHandler WindowsChanged;

		public ApplicationInfo Info { get; }

		internal ExternalApplication(string executablePath, ApplicationInfo info, IModuleLogger logger, IProcessFactory processFactory)
		{
			this.executablePath = executablePath;
			this.Info = info;
			this.logger = logger;
			this.instances = new List<ExternalApplicationInstance>();
			this.processFactory = processFactory;
		}

		public IEnumerable<IApplicationWindow> GetWindows()
		{
			return Enumerable.Empty<IApplicationWindow>();
		}

		public void Initialize()
		{
			// Nothing to do here for now.
		}

		public void Start()
		{
			try
			{
				logger.Info("Starting application...");

				var process = processFactory.StartNew(executablePath);
				var id = new ApplicationInstanceIdentifier(process.Id);
				var instance = new ExternalApplicationInstance(Info.Icon, id, logger.CloneFor($"{Info.Name} {id}"), process);

				instance.Initialize();
				instances.Add(instance);
			}
			catch (Exception e)
			{
				logger.Error("Failed to start application!", e);
			}
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
