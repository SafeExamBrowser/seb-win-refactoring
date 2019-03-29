/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.WindowsApi;

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
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
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
			ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout,
			ISystemComponent<ISystemPowerSupplyControl> powerSupply,
			ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork,
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
			this.keyboardLayout = keyboardLayout;
			this.powerSupply = powerSupply;
			this.systemInfo = systemInfo;
			this.taskbarSettings = taskbarSettings;
			this.terminationActivator = terminationActivator;
			this.text = text;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
			this.wirelessNetwork = wirelessNetwork;
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
		}

		private void InitializeActionCenter()
		{
			if (actionCenterSettings.EnableActionCenter)
			{
				logger.Info("Initializing action center...");
				actionCenter.InitializeText(text);

				InitializeAboutNotificationForActionCenter();
				InitializeClockForActionCenter();
				InitializeLogNotificationForActionCenter();
				InitializeKeyboardLayoutForActionCenter();
				InitializeWirelessNetworkForActionCenter();
				InitializePowerSupplyForActionCenter();

				foreach (var activator in activators)
				{
					actionCenter.Register(activator);
					activator.Start();
				}
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
				InitializeClockForTaskbar();
				InitializeLogNotificationForTaskbar();
				InitializeKeyboardLayoutForTaskbar();
				InitializeWirelessNetworkForTaskbar();
				InitializePowerSupplyForTaskbar();
			}
			else
			{
				logger.Info("Taskbar is disabled, skipping initialization.");
			}
		}

		private void InitializeSystemComponents()
		{
			keyboardLayout.Initialize();
			powerSupply.Initialize();
			wirelessNetwork.Initialize();
		}

		private void InitializeAboutNotificationForActionCenter()
		{
			var notification = uiFactory.CreateNotificationControl(aboutInfo, Location.ActionCenter);

			aboutController.RegisterNotification(notification);
			actionCenter.AddNotificationControl(notification);
		}

		private void InitializeAboutNotificationForTaskbar()
		{
			var notification = uiFactory.CreateNotificationControl(aboutInfo, Location.Taskbar);

			aboutController.RegisterNotification(notification);
			taskbar.AddNotificationControl(notification);
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

				logController.RegisterNotification(notification);
				actionCenter.AddNotificationControl(notification);
			}
		}

		private void InitializeLogNotificationForTaskbar()
		{
			if (taskbarSettings.ShowApplicationLog)
			{
				var notification = uiFactory.CreateNotificationControl(logInfo, Location.Taskbar);

				logController.RegisterNotification(notification);
				taskbar.AddNotificationControl(notification);
			}
		}

		private void InitializeKeyboardLayoutForActionCenter()
		{
			if (actionCenterSettings.ShowKeyboardLayout)
			{
				var control = uiFactory.CreateKeyboardLayoutControl(Location.ActionCenter);

				keyboardLayout.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializeKeyboardLayoutForTaskbar()
		{
			if (taskbarSettings.ShowKeyboardLayout)
			{
				var control = uiFactory.CreateKeyboardLayoutControl(Location.Taskbar);

				keyboardLayout.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void InitializePowerSupplyForActionCenter()
		{
			if (systemInfo.HasBattery)
			{
				var control = uiFactory.CreatePowerSupplyControl(Location.ActionCenter);

				powerSupply.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializePowerSupplyForTaskbar()
		{
			if (systemInfo.HasBattery)
			{
				var control = uiFactory.CreatePowerSupplyControl(Location.Taskbar);

				powerSupply.Register(control);
				taskbar.AddSystemControl(control);
			}
		}

		private void InitializeWirelessNetworkForActionCenter()
		{
			if (actionCenterSettings.ShowWirelessNetwork)
			{
				var control = uiFactory.CreateWirelessNetworkControl(Location.ActionCenter);

				wirelessNetwork.Register(control);
				actionCenter.AddSystemControl(control);
			}
		}

		private void InitializeWirelessNetworkForTaskbar()
		{
			if (taskbarSettings.ShowWirelessNetwork)
			{
				var control = uiFactory.CreateWirelessNetworkControl(Location.Taskbar);

				wirelessNetwork.Register(control);
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
			keyboardLayout.Terminate();
			powerSupply.Terminate();
			wirelessNetwork.Terminate();
		}
	}
}
