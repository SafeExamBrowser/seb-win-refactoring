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
		private int instanceIdCounter = default(int);

		private string executablePath;
		private IModuleLogger logger;
		private INativeMethods nativeMethods;
		private IList<ExternalApplicationInstance> instances;
		private IProcessFactory processFactory;

		public event WindowsChangedEventHandler WindowsChanged;

		public ApplicationInfo Info { get; }

		internal ExternalApplication(
			string executablePath,
			ApplicationInfo info,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IProcessFactory processFactory)
		{
			this.executablePath = executablePath;
			this.Info = info;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.instances = new List<ExternalApplicationInstance>();
			this.processFactory = processFactory;
		}

		public IEnumerable<IApplicationWindow> GetWindows()
		{
			return instances.SelectMany(i => i.GetWindows());
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
				var id = ++instanceIdCounter;
				var instanceLogger = logger.CloneFor($"{Info.Name} Instance #{id}");
				var instance = new ExternalApplicationInstance(Info.Icon, id, instanceLogger, nativeMethods, process);

				instance.Initialize();
				instance.Terminated += Instance_Terminated;
				instance.WindowsChanged += () => WindowsChanged?.Invoke();
				instances.Add(instance);
			}
			catch (Exception e)
			{
				logger.Error("Failed to start application!", e);
			}
		}

		private void Instance_Terminated(int id)
		{
			instances.Remove(instances.First(i => i.Id == id));
			WindowsChanged?.Invoke();
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
