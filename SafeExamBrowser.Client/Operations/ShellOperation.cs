/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class ShellOperation : IOperation
	{
		private IActionCenter actionCenter;
		private IEnumerable<IActionCenterActivator> activators;
		private ActionCenterSettings actionCenterSettings;
		private ILogger logger;
		private INotificationInfo aboutInfo;
		private INotificationController aboutController;
		private INotificationInfo logInfo;
		private INotificationController logController;
		// TODO private ISystemComponent<ISystemAudioControl> audio;
		// TODO private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		// TODO private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		// TODO private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private TaskbarSettings taskbarSettings;
		private ITerminationActivator terminationActivator;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ShellOperation(
			IActionCenter actionCenter,
			IEnumerable<IActionCenterActivator> activators,
			ActionCenterSettings actionCenterSettings,
			ILogger logger,
			INotificationInfo aboutInfo,
			INotificationController aboutController,
			INotificationInfo logInfo,
			INotificationController logController,
			// TODO ISystemComponent<ISystemAudioControl> audio,
			// TODO ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout,
			// TODO ISystemComponent<ISystemPowerSupplyControl> powerSupply,
			// TODO ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork,
			ISystemInfo systemInfo,
			ITaskbar taskbar,
			TaskbarSettings taskbarSettings,
			ITerminationActivator terminationActivator,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.aboutInfo = aboutInfo;
			this.aboutController = aboutController;
			this.actionCenter = actionCenter;
			this.activators = activators;
			this.actionCenterSettings = actionCenterSettings;
			this.logger = logger;
			this.logInfo = logInfo;
			this.logController = logController;
			// TODO this.audio = audio;
			// TODO this.keyboardLayout = keyboardLayout;
			// TODO this.powerSupply = powerSupply;
			this.systemInfo = systemInfo;
			this.taskbarSettings = taskbarSettings;
			this.terminationActivator = terminationActivator;
			this.text = text;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
			// TODO this.wirelessNetwork = wirelessNetwork;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing shell...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeShell);

			InitializeSystemComponents();
			InitializeActionCenter();
			InitializeTaskbar();
			InitializeActivators();

			return OperationResult.Success;
		}

		public OperationResult Revert()
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
			terminationActivator.Start();

			if (actionCenterSettings.EnableActionCenter)
			{
				foreach (var activator in activators)
				{
					actionCenter.Register(activator);
					activator.Start();
				}
			}
		}

		private void InitializeActionCenter()
		{
			if (actionCenterSettings.EnableActionCenter)
			{
				logger.Info("Initializing action center...");
				actionCenter.InitializeText(text);

				InitializeAboutNotificationForActionCenter();
				InitializeAudioForActionCenter();
				InitializeClockForActionCenter();
				InitializeLogNotificationForActionCenter();
				InitializeKeyboardLayoutForActionCenter();
				InitializeWirelessNetworkForActionCenter();
				InitializePowerSupplyForActionCenter();
			}
			else
			{
				logger.Info("Action center is disabled, skipping initialization.");
			}
		}

		private void InitializeTaskbar()
		{
			if (taskbarSettings.EnableTaskbar)
			{
				logger.Info("Initializing taskbar...");
				taskbar.InitializeText(text);

				InitializeAboutNotificationForTaskbar();
				InitializeLogNotificationForTaskbar();
				InitializePowerSupplyForTaskbar();
				InitializeWirelessNetworkForTaskbar();
				InitializeAudioForTaskbar();
				InitializeKeyboardLayoutForTaskbar();
				InitializeClockForTaskbar();
			}
			else
			{
				logger.Info("Taskbar is disabled, skipping initialization.");
			}
		}

		private void InitializeSystemComponents()
		{
			// TODO 
			//audio.Initialize();
			//keyboardLayout.Initialize();
			//powerSupply.Initialize();
			//wirelessNetwork.Initialize();
		}

		private void InitializeAboutNotificationForActionCenter()
		{
			if (actionCenterSettings.ShowApplicationInfo)
			{
				var notification = uiFactory.CreateNotificationControl(aboutInfo, Location.ActionCenter);

				// TODO aboutController.RegisterNotification(notification);
				actionCenter.AddNotificationControl(notification);
			}
		}

		private void InitializeAboutNotificationForTaskbar()
		{
			if (taskbarSettings.ShowApplicationInfo)
			{
				var notification = uiFactory.CreateNotificationControl(aboutInfo, Location.Taskbar);

				// TODO aboutController.RegisterNotification(notification);
				taskbar.AddNotificationControl(notification);
			}
		}

		private void InitializeAudioForActionCenter()
		{
			if (actionCenterSettings.ShowAudio)
			{
				var control = uiFactory.CreateAudioControl(Location.ActionCenter);

				// TODO audio.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializeAudioForTaskbar()
		{
			if (taskbarSettings.ShowAudio)
			{
				var control = uiFactory.CreateAudioControl(Location.Taskbar);

				// TODO audio.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void InitializeClockForActionCenter()
		{
			actionCenter.ShowClock = actionCenterSettings.ShowClock;
		}

		private void InitializeClockForTaskbar()
		{
			taskbar.ShowClock = taskbarSettings.ShowClock;
		}

		private void InitializeLogNotificationForActionCenter()
		{
			if (actionCenterSettings.ShowApplicationLog)
			{
				var notification = uiFactory.CreateNotificationControl(logInfo, Location.ActionCenter);

				// TODO logController.RegisterNotification(notification);
				actionCenter.AddNotificationControl(notification);
			}
		}

		private void InitializeLogNotificationForTaskbar()
		{
			if (taskbarSettings.ShowApplicationLog)
			{
				var notification = uiFactory.CreateNotificationControl(logInfo, Location.Taskbar);

				// TODO logController.RegisterNotification(notification);
				taskbar.AddNotificationControl(notification);
			}
		}

		private void InitializeKeyboardLayoutForActionCenter()
		{
			if (actionCenterSettings.ShowKeyboardLayout)
			{
				var control = uiFactory.CreateKeyboardLayoutControl(Location.ActionCenter);

				// TODO keyboardLayout.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializeKeyboardLayoutForTaskbar()
		{
			if (taskbarSettings.ShowKeyboardLayout)
			{
				var control = uiFactory.CreateKeyboardLayoutControl(Location.Taskbar);

				// TODO keyboardLayout.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void InitializePowerSupplyForActionCenter()
		{
			if (systemInfo.HasBattery)
			{
				var control = uiFactory.CreatePowerSupplyControl(Location.ActionCenter);

				// TODO powerSupply.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializePowerSupplyForTaskbar()
		{
			if (systemInfo.HasBattery)
			{
				var control = uiFactory.CreatePowerSupplyControl(Location.Taskbar);

				// TODO powerSupply.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void InitializeWirelessNetworkForActionCenter()
		{
			if (actionCenterSettings.ShowWirelessNetwork)
			{
				var control = uiFactory.CreateWirelessNetworkControl(Location.ActionCenter);

				// TODO wirelessNetwork.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializeWirelessNetworkForTaskbar()
		{
			if (taskbarSettings.ShowWirelessNetwork)
			{
				var control = uiFactory.CreateWirelessNetworkControl(Location.Taskbar);

				// TODO wirelessNetwork.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void TerminateActivators()
		{
			terminationActivator.Stop();

			if (actionCenterSettings.EnableActionCenter)
			{
				foreach (var activator in activators)
				{
					activator.Stop();
				}
			}
		}

		private void TerminateNotifications()
		{
			aboutController.Terminate();
			logController.Terminate();
		}

		private void TerminateSystemComponents()
		{
			// TODO 
			//audio.Terminate();
			//keyboardLayout.Terminate();
			//powerSupply.Terminate();
			//wirelessNetwork.Terminate();
		}
	}
}
