/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Keyboard;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Monitoring.Keyboard
{
	public class KeyboardInterceptor : IKeyboardInterceptor
	{
		private Guid? hookId;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private KeyboardSettings settings;

		public KeyboardInterceptor(ILogger logger, INativeMethods nativeMethods, KeyboardSettings settings)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.settings = settings;
		}

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
			var block = false;
			var key = KeyInterop.KeyFromVirtualKey(keyCode);

			block |= key == Key.Apps;
			block |= key == Key.Escape && modifier == KeyModifier.None && !settings.AllowEsc;
			block |= key == Key.F1 && !settings.AllowF1;
			block |= key == Key.F2 && !settings.AllowF2;
			block |= key == Key.F3 && !settings.AllowF3;
			block |= key == Key.F4 && !settings.AllowF4;
			block |= key == Key.F5 && !settings.AllowF5;
			block |= key == Key.F6 && !settings.AllowF6;
			block |= key == Key.F7 && !settings.AllowF7;
			block |= key == Key.F8 && !settings.AllowF8;
			block |= key == Key.F9 && !settings.AllowF9;
			block |= key == Key.F10 && !settings.AllowF10;
			block |= key == Key.F11 && !settings.AllowF11;
			block |= key == Key.F12 && !settings.AllowF12;
			block |= key == Key.LWin && !settings.AllowSystemKey;
			block |= key == Key.PrintScreen && !settings.AllowPrintScreen;
			block |= key == Key.RWin && !settings.AllowSystemKey;
			block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Escape && !settings.AllowAltEsc;
			block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.F4 && !settings.AllowAltF4;
			block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Space;
			block |= modifier.HasFlag(KeyModifier.Alt) && key == Key.Tab;
			block |= modifier.HasFlag(KeyModifier.Ctrl) && key == Key.Escape && !settings.AllowCtrlEsc;

			if (block)
			{
				Log(key, keyCode, modifier, state);
			}

			return block;
		}

		private void Log(Key key, int keyCode, KeyModifier modifier, KeyState state)
		{
			var modifierFlags = Enum.GetValues(typeof(KeyModifier)).OfType<KeyModifier>().Where(m => m != KeyModifier.None && modifier.HasFlag(m));
			var modifiers = modifierFlags.Any() ? String.Join(" + ", modifierFlags) + " + " : string.Empty;

			logger.Info($"Blocked '{modifiers}{key}' ({key} = {keyCode}) when {state.ToString().ToLower()}.");
		}
	}
}
