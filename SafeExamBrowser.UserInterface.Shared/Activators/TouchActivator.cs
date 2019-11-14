/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Shared.Activators
{
	public abstract class TouchActivator
	{
		private INativeMethods nativeMethods;
		private Guid? hookId;

		protected bool Paused { get; set; }

		protected TouchActivator(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
		}

		protected abstract bool Process(MouseButton button, MouseButtonState state, MouseInformation info);

		public void Start()
		{
			hookId = nativeMethods.RegisterMouseHook(MouseHookCallback);
		}

		public void Stop()
		{
			if (hookId.HasValue)
			{
				nativeMethods.DeregisterMouseHook(hookId.Value);
			}
		}

		private bool MouseHookCallback(MouseButton button, MouseButtonState state, MouseInformation info)
		{
			if (!Paused && info.IsTouch)
			{
				return Process(button, state, info);
			}

			return false;
		}
	}
}
