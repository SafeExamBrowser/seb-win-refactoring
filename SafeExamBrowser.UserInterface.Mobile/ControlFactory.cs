/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Mobile
{
	internal class ControlFactory
	{
		private readonly IText text;

		internal ControlFactory(IText text)
		{
			this.text = text;
		}

		internal IApplicationControl CreateApplicationControl(IApplication<IApplicationWindow> application, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.ApplicationControl(application);
			}
			else
			{
				return new Controls.Taskbar.ApplicationControl(application);
			}
		}

		internal ISystemControl CreateAudioControl(IAudio audio, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.AudioControl(audio, text);
			}
			else
			{
				return new Controls.Taskbar.AudioControl(audio, text);
			}
		}

		internal ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.KeyboardLayoutControl(keyboard, text);
			}
			else
			{
				return new Controls.Taskbar.KeyboardLayoutControl(keyboard, text);
			}
		}

		internal ISystemControl CreateNetworkControl(INetworkAdapter adapter, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.NetworkControl(adapter, text);
			}
			else
			{
				return new Controls.Taskbar.NetworkControl(adapter, text);
			}
		}

		internal INotificationControl CreateNotificationControl(INotification notification, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.NotificationButton(notification);
			}
			else
			{
				return new Controls.Taskbar.NotificationButton(notification);
			}
		}

		internal ISystemControl CreatePowerSupplyControl(IPowerSupply powerSupply, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.PowerSupplyControl(powerSupply, text);
			}
			else
			{
				return new Controls.Taskbar.PowerSupplyControl(powerSupply, text);
			}
		}

		internal INotificationControl CreateRaiseHandControl(IProctoringController controller, Location location, ProctoringSettings settings)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.RaiseHandControl(controller, settings, text);
			}
			else
			{
				return new Controls.Taskbar.RaiseHandControl(controller, settings, text);
			}
		}
	}
}
