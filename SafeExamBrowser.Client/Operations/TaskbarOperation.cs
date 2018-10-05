/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.Operations
{
	internal class TaskbarOperation : IOperation
	{
		private ILogger logger;
		private INotificationInfo logInfo;
		private INotificationController logController;
		private TaskbarSettings settings;
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private IUserInterfaceFactory uiFactory;
		private IText text;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public TaskbarOperation(
			ILogger logger,
			INotificationInfo logInfo,
			INotificationController logController,
			ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout,
			ISystemComponent<ISystemPowerSupplyControl> powerSupply,
			ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork,
			ISystemInfo systemInfo,
			ITaskbar taskbar,
			TaskbarSettings settings,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.logInfo = logInfo;
			this.logController = logController;
			this.keyboardLayout = keyboardLayout;
			this.powerSupply = powerSupply;
			this.settings = settings;
			this.systemInfo = systemInfo;
			this.taskbar = taskbar;
			this.text = text;
			this.uiFactory = uiFactory;
			this.wirelessNetwork = wirelessNetwork;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing taskbar...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeTaskbar);

			if (settings.AllowApplicationLog)
			{
				CreateLogNotification();
			}

			if (settings.AllowKeyboardLayout)
			{
				AddKeyboardLayoutControl();
			}

			if (systemInfo.HasBattery)
			{
				AddPowerSupplyControl();
			}

			if (settings.AllowWirelessNetwork)
			{
				AddWirelessNetworkControl();
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			throw new InvalidOperationException($"The '{nameof(TaskbarOperation)}' is not meant to be repeated!");
		}

		public void Revert()
		{
			logger.Info("Terminating taskbar...");
			StatusChanged?.Invoke(TextKey.OperationStatus_TerminateTaskbar);

			if (settings.AllowApplicationLog)
			{
				logController.Terminate();
			}

			if (settings.AllowKeyboardLayout)
			{
				keyboardLayout.Terminate();
			}

			if (systemInfo.HasBattery)
			{
				powerSupply.Terminate();
			}

			if (settings.AllowWirelessNetwork)
			{
				wirelessNetwork.Terminate();
			}
		}

		private void AddKeyboardLayoutControl()
		{
			var control = uiFactory.CreateKeyboardLayoutControl();

			keyboardLayout.Initialize(control);
			taskbar.AddSystemControl(control);
		}

		private void AddPowerSupplyControl()
		{
			var control = uiFactory.CreatePowerSupplyControl();

			powerSupply.Initialize(control);
			taskbar.AddSystemControl(control);
		}

		private void AddWirelessNetworkControl()
		{
			var control = uiFactory.CreateWirelessNetworkControl();

			wirelessNetwork.Initialize(control);
			taskbar.AddSystemControl(control);
		}

		private void CreateLogNotification()
		{
			var logNotification = uiFactory.CreateNotification(logInfo);
			
			logController.RegisterNotification(logNotification);
			taskbar.AddNotification(logNotification);
		}
	}
}
