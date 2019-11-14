/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Input;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Shared.Activators
{
	public abstract class KeyboardActivator
	{
		private INativeMethods nativeMethods;
		private Guid? hookId;

		protected bool Paused { get; set; }

		protected KeyboardActivator(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
		}

		protected abstract bool Process(Key key, KeyModifier modifier, KeyState state);

		public void Start()
		{
			hookId = nativeMethods.RegisterKeyboardHook(KeyboardHookCallback);
		}

		public void Stop()
		{
			if (hookId.HasValue)
			{
				nativeMethods.DeregisterKeyboardHook(hookId.Value);
			}
		}

		private bool KeyboardHookCallback(int keyCode, KeyModifier modifier, KeyState state)
		{
			if (!Paused)
			{
				return Process(KeyInterop.KeyFromVirtualKey(keyCode), modifier, state);
			}

			return false;
		}
	}
}
