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
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ActionCenterOperation : IOperation
	{
		private IActionCenter actionCenter;
		private IEnumerable<IActionCenterActivator> activators;
		private ILogger logger;
		private INotificationInfo aboutInfo;
		private INotificationController aboutController;
		private INotificationInfo logInfo;
		private INotificationController logController;
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ActionCenterSettings settings;
		private ISystemInfo systemInfo;
		private IUserInterfaceFactory uiFactory;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ActionCenterOperation(
			IActionCenter actionCenter,
			IEnumerable<IActionCenterActivator> activators,
			ILogger logger,
			INotificationInfo aboutInfo,
			INotificationController aboutController,
			INotificationInfo logInfo,
			INotificationController logController,
			ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout,
			ISystemComponent<ISystemPowerSupplyControl> powerSupply,
			ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork,
			ActionCenterSettings settings,
			ISystemInfo systemInfo,
			IUserInterfaceFactory uiFactory)
		{
			this.actionCenter = actionCenter;
			this.activators = activators;
			this.logger = logger;
			this.aboutInfo = aboutInfo;
			this.aboutController = aboutController;
			this.logInfo = logInfo;
			this.logController = logController;
			this.keyboardLayout = keyboardLayout;
			this.powerSupply = powerSupply;
			this.wirelessNetwork = wirelessNetwork;
			this.systemInfo = systemInfo;
			this.settings = settings;
			this.uiFactory = uiFactory;
		}

		public OperationResult Perform()
		{
			foreach (var activator in activators)
			{
				actionCenter.Register(activator);
				activator.Start();
			}

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			foreach (var activator in activators)
			{
				activator.Stop();
			}

			return OperationResult.Success;
		}
	}
}
