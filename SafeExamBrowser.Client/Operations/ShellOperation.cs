/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ShellOperation : ClientOperation
	{
		private IActionCenter actionCenter;
		private IAudio audio;
		private INotificationInfo aboutInfo;
		private INotificationController aboutController;
		private IKeyboard keyboard;
		private ILogger logger;
		private INotificationInfo logInfo;
		private INotificationController logController;
		private IPowerSupply powerSupply;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private ITaskview taskview;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private IWirelessAdapter wirelessAdapter;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ShellOperation(
			IActionCenter actionCenter,
			IAudio audio,
			INotificationInfo aboutInfo,
			INotificationController aboutController,
			ClientContext context,
			IKeyboard keyboard,
			ILogger logger,
			INotificationInfo logInfo,
			INotificationController logController,
			IPowerSupply powerSupply,
			ISystemInfo systemInfo,
			ITaskbar taskbar,
			ITaskview taskview,
			IText text,
			IUserInterfaceFactory uiFactory,
			IWirelessAdapter wirelessAdapter) : base(context)
		{
			this.aboutInfo = aboutInfo;
			this.aboutController = aboutController;
			this.actionCenter = actionCenter;
			this.audio = audio;
			this.keyboard = keyboard;
			this.logger = logger;
			this.logInfo = logInfo;
			this.logController = logController;
			this.powerSupply = powerSupply;
			this.systemInfo = systemInfo;
			this.text = text;
			this.taskbar = taskbar;
			this.taskview = taskview;
			this.uiFactory = uiFactory;
			this.wirelessAdapter = wirelessAdapter;
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
				InitializeWirelessNetworkForActionCenter();
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
				InitializeWirelessNetworkForTaskbar();
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
			powerSupply.Initialize();
			wirelessAdapter.Initialize();
		}

		private void InitializeAboutNotificationForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowApplicationInfo)
			{
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(aboutController, aboutInfo, Location.ActionCenter));
			}
		}

		private void InitializeAboutNotificationForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowApplicationInfo)
			{
				taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(aboutController, aboutInfo, Location.Taskbar));
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
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(logController, logInfo, Location.ActionCenter));
			}
		}

		private void InitializeLogNotificationForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowApplicationLog)
			{
				taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(logController, logInfo, Location.Taskbar));
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

		private void InitializeWirelessNetworkForActionCenter()
		{
			if (Context.Settings.ActionCenter.ShowWirelessNetwork)
			{
				actionCenter.AddSystemControl(uiFactory.CreateWirelessNetworkControl(wirelessAdapter, Location.ActionCenter));
			}
		}

		private void InitializeWirelessNetworkForTaskbar()
		{
			if (Context.Settings.Taskbar.ShowWirelessNetwork)
			{
				taskbar.AddSystemControl(uiFactory.CreateWirelessNetworkControl(wirelessAdapter, Location.Taskbar));
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
			aboutController.Terminate();
			logController.Terminate();
		}

		private void TerminateSystemComponents()
		{
			audio.Terminate();
			keyboard.Terminate();
			powerSupply.Terminate();
			wirelessAdapter.Terminate();
		}
	}
}
