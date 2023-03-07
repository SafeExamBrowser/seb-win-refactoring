/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
		private Guid? hookId;
		private INativeMethods nativeMethods;
		protected bool paused;

		protected TouchActivator(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
		}

		protected abstract void OnBeforeResume();
		protected abstract bool Process(MouseButton button, MouseButtonState state, MouseInformation info);

		public void Pause()
		{
			paused = true;
		}

		public void Resume()
		{
			OnBeforeResume();
			paused = false;
		}

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
			if (!paused && info.IsTouch)
			{
				return Process(button, state, info);
			}

			return false;
		}
	}
}
