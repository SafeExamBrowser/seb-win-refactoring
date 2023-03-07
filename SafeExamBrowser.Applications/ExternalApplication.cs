/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplication : IApplication
	{
		private readonly object @lock = new object();

		private IApplicationMonitor applicationMonitor;
		private string executablePath;
		private IModuleLogger logger;
		private INativeMethods nativeMethods;
		private IList<ExternalApplicationInstance> instances;
		private IProcessFactory processFactory;
		private WhitelistApplication settings;

		public bool AutoStart { get; private set; }
		public IconResource Icon { get; private set; }
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Tooltip { get; private set; }

		public event WindowsChangedEventHandler WindowsChanged;

		internal ExternalApplication(
			IApplicationMonitor applicationMonitor,
			string executablePath,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IProcessFactory processFactory,
			WhitelistApplication settings)
		{
			this.applicationMonitor = applicationMonitor;
			this.executablePath = executablePath;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.instances = new List<ExternalApplicationInstance>();
			this.processFactory = processFactory;
			this.settings = settings;
		}

		public IEnumerable<IApplicationWindow> GetWindows()
		{
			lock (@lock)
			{
				return instances.SelectMany(i => i.GetWindows());
			}
		}

		public void Initialize()
		{
			AutoStart = settings.AutoStart;
			Icon = new EmbeddedIconResource { FilePath = executablePath };
			Id = settings.Id;
			Name = settings.DisplayName;
			Tooltip = settings.Description ?? settings.DisplayName;

			applicationMonitor.InstanceStarted += ApplicationMonitor_InstanceStarted;
		}

		public void Start()
		{
			try
			{
				logger.Info("Starting application...");
				InitializeInstance(processFactory.StartNew(executablePath, BuildArguments()));
				logger.Info("Successfully started application.");
			}
			catch (Exception e)
			{
				logger.Error("Failed to start application!", e);
			}
		}

		private string[] BuildArguments()
		{
			var arguments = new List<string>();

			foreach (var argument in settings.Arguments)
			{
				arguments.Add(Environment.ExpandEnvironmentVariables(argument));
			}

			return arguments.ToArray();
		}

		public void Terminate()
		{
			applicationMonitor.InstanceStarted -= ApplicationMonitor_InstanceStarted;

			try
			{
				lock (@lock)
				{
					if (instances.Any() && !settings.AllowRunning)
					{
						logger.Info($"Terminating application with {instances.Count} instance(s)...");

						foreach (var instance in instances)
						{
							instance.Terminated -= Instance_Terminated;
							instance.Terminate();
						}

						logger.Info("Successfully terminated application.");
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to terminate application!", e);
			}
		}

		private void ApplicationMonitor_InstanceStarted(Guid applicationId, IProcess process)
		{
			lock (@lock)
			{
				var isNewInstance = instances.All(i => i.Id != process.Id);

				if (applicationId == Id && isNewInstance)
				{
					logger.Info("New application instance was started.");
					InitializeInstance(process);
				}
			}
		}

		private void Instance_Terminated(int id)
		{
			lock (@lock)
			{
				instances.Remove(instances.First(i => i.Id == id));
			}

			WindowsChanged?.Invoke();
		}

		private void InitializeInstance(IProcess process)
		{
			lock (@lock)
			{
				var instanceLogger = logger.CloneFor($"{Name} ({process.Id})");
				var instance = new ExternalApplicationInstance(Icon, instanceLogger, nativeMethods, process);

				instance.Terminated += Instance_Terminated;
				instance.WindowsChanged += () => WindowsChanged?.Invoke();
				instance.Initialize();

				instances.Add(instance);
			}
		}
	}
}
