/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ShellOperation : ClientOperation
	{
		private readonly IActionCenter actionCenter;
		private readonly IAudio audio;
		private readonly INotification aboutNotification;
		private readonly IKeyboard keyboard;
		private readonly ILogger logger;
		private readonly INotification logNotification;
		private readonly INetworkAdapter networkAdapter;
		private readonly IPowerSupply powerSupply;
		private readonly ISystemInfo systemInfo;
		private readonly ITaskbar taskbar;
		private readonly ITaskview taskview;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ShellOperation(
			IActionCenter actionCenter,
			IAudio audio,
			INotification aboutNotification,
			ClientContext context,
			IKeyboard keyboard,
			ILogger logger,
			INotification logNotification,
			INetworkAdapter networkAdapter,
			IPowerSupply powerSupply,
			ISystemInfo systemInfo,
			ITaskbar taskbar,
			ITaskview taskview,
			IText text,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.aboutNotification = aboutNotification;
			this.actionCenter = actionCenter;
			this.audio = audio;
			this.keyboard = keyboard;
			this.logger = logger;
			this.logNotification = logNotification;
			this.networkAdapter = networkAdapter;
			this.powerSupply = powerSupply;
			this.systemInfo = systemInfo;
			this.text = text;
			this.taskbar = taskbar;
			this.taskview = taskview;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing shell...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeShell);

			InitializeSystemComponents();
			InitializeActionCenter();
			InitializeTaskbar();
			InitializeTaskview();
			InitializeActivators();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Terminating shell...");
			StatusChanged?.Invoke(TextKey.OperationStatus_TerminateShell);

			TerminateActivators();
			TerminateNotifications();
			TerminateSystemComponents();

			return OperationResult.Success;
		}

		private void InitializeActivators()
		{
			foreach (var activator in Context.Activators)
			{
				if (Context.Settings.ActionCenter.EnableActionCenter && activator is IActionCenterActivator actionCenterActivator)
				{
					actionCenter.Register(actionCenterActivator);
					actionCenterActivator.Start();
				}

				if (Context.Settings.Keyboard.AllowAltTab && activator is ITaskviewActivator taskViewActivator)
				{
					taskview.Register(taskViewActivator);
					taskViewActivator.Start();
				}

				if (Context.Settings.Security.AllowTermination && activator is ITerminationActivator terminationActivator)
				{
					terminationActivator.Start();
				}

				if (Context.Settings.Taskbar.EnableTaskbar && activator is ITaskbarActivator taskbarActivator)
				{
					taskbar.Register(taskbarActivator);
					taskbarActivator.Start();
				}
			}
		}

		private void InitializeActionCenter()
		{
			if (Context.Settings.ActionCenter.EnableActionCenter)
			{
				logger.Info("Initializing action center...");
				actionCenter.InitializeText(text);

				InitializeApplicationsFor(Location.ActionCenter);
				InitializeAboutNotificationForActionCenter();
				InitializeAudioForActionCenter();
				InitializeClockForActionCenter();
				InitializeLogNotificationForActionCenter();
				InitializeKeyboardLayoutForActionCenter();
				InitializeNetworkForActionCenter();
				InitializePowerSupplyForActionCenter();
				InitializeQuitButtonForActionCenter();
			}
			else
			{
				logger.Info("Action center is disabled, skipping initialization.");
			}
		}

		private void InitializeTaskbar()
		{
			if (Context.Settings.Taskbar.EnableTaskbar)
			{
				logger.Info("Initializing taskbar...");
				taskbar.InitializeText(text);

				InitializeApplicationsFor(Location.Taskbar);
				InitializeAboutNotificationForTaskbar();
				InitializeLogNotificationForTaskbar();
				InitializePowerSupplyForTaskbar();
				InitializeNetworkForTaskbar();
				InitializeAudioForTaskbar();
				InitializeKeyboardLayoutForTaskbar();
				InitializeClockForTaskbar();
				InitializeQuitButtonForTaskbar();
			}
			else
			{
				logger.Info("Taskbar is disabled, skipping initialization.");
			}
		}

		private void InitializeTaskview()
		{
			logger.Info("Initializing task view...");

			foreach (var application in Context.Applications)
			{
				taskview.Add(application);
			}
		}

		private void InitializeApplicationsFor(Location location)
		{
			foreach (var application in Context.Applications)
			{
				var settings = Context.Settings.Applications.Whitelist.First(a => a.Id == application.Id);

				if (settings.ShowInShell)
				{
					var control = uiFactory.CreateApplicationControl(application, location);

					switch (location)
					{
						case Location.ActionCenter:
							actionCenter.AddApplicationControl(control);
							break;
						case Location.Taskbar:
							taskbar.AddApplicationControl(control);
							break;
					}
				}
			}
		}

		private void InitializeSystemComponents()
		{
			audio.Initialize();
			keyboard.Initialize();
			networkAdapter.Initialize();
			powerSupply.Initialize();
		}

		private void InitializeAboutNotificationForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowApplicationInfo)
			{
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(aboutNotification, Location.ActionCenter));
			}
		}

		private void InitializeAboutNotificationForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowApplicationInfo)
			{
				taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(aboutNotification, Location.Taskbar));
			}
		}

		private void InitializeAudioForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowAudio)
			{
				actionCenter.AddSystemControl(uiFactory.CreateAudioControl(audio, Location.ActionCenter));
			}
		}

		private void InitializeAudioForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowAudio)
			{
				taskbar.AddSystemControl(uiFactory.CreateAudioControl(audio, Location.Taskbar));
			}
		}

		private void InitializeClockForActionCenter()
		{
			actionCenter.ShowClock = Context.Settings.ActionCenter.ShowClock;
		}

		private void InitializeClockForTaskbar()
		{
			taskbar.ShowClock = Context.Settings.Taskbar.ShowClock;
		}

		private void InitializeLogNotificationForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowApplicationLog)
			{
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(logNotification, Location.ActionCenter));
			}
		}

		private void InitializeLogNotificationForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowApplicationLog)
			{
				taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(logNotification, Location.Taskbar));
			}
		}

		private void InitializeKeyboardLayoutForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowKeyboardLayout)
			{
				actionCenter.AddSystemControl(uiFactory.CreateKeyboardLayoutControl(keyboard, Location.ActionCenter));
			}
		}

		private void InitializeKeyboardLayoutForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowKeyboardLayout)
			{
				taskbar.AddSystemControl(uiFactory.CreateKeyboardLayoutControl(keyboard, Location.Taskbar));
			}
		}

		private void InitializePowerSupplyForActionCenter()
		{
			if (systemInfo.HasBattery)
			{
				actionCenter.AddSystemControl(uiFactory.CreatePowerSupplyControl(powerSupply, Location.ActionCenter));
			}
		}

		private void InitializePowerSupplyForTaskbar()
		{
			if (systemInfo.HasBattery)
			{
				taskbar.AddSystemControl(uiFactory.CreatePowerSupplyControl(powerSupply, Location.Taskbar));
			}
		}

		private void InitializeQuitButtonForActionCenter()
		{
			actionCenter.ShowQuitButton = Context.Settings.Security.AllowTermination;
		}

		private void InitializeQuitButtonForTaskbar()
		{
			taskbar.ShowQuitButton = Context.Settings.Security.AllowTermination;
		}

		private void InitializeNetworkForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowNetwork)
			{
				actionCenter.AddSystemControl(uiFactory.CreateNetworkControl(networkAdapter, Location.ActionCenter));
			}
		}

		private void InitializeNetworkForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowNetwork)
			{
				taskbar.AddSystemControl(uiFactory.CreateNetworkControl(networkAdapter, Location.Taskbar));
			}
		}

		private void TerminateActivators()
		{
			foreach (var activator in Context.Activators)
			{
				activator.Stop();
			}
		}

		private void TerminateNotifications()
		{
			aboutNotification.Terminate();
			logNotification.Terminate();
		}

		private void TerminateSystemComponents()
		{
			audio.Terminate();
			keyboard.Terminate();
			networkAdapter.Terminate();
			powerSupply.Terminate();
		}
	}
}
