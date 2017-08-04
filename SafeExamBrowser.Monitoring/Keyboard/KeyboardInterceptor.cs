/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Windows.Input;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;

namespace SafeExamBrowser.Monitoring.Keyboard
{
	public class KeyboardInterceptor : IKeyboardInterceptor
	{
		private IKeyboardSettings settings;
		private ILogger logger;

		public KeyboardInterceptor(IKeyboardSettings settings, ILogger logger)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public bool Block(int keyCode, KeyModifier modifier, KeyState state)
		{
			var block = false;
			var key = KeyInterop.KeyFromVirtualKey(keyCode);

			block |= key == Key.Apps;
			block |= key == Key.F1;
			block |= key == Key.F2;
			block |= key == Key.F3;
			block |= key == Key.F4;
			block |= key == Key.F6;
			block |= key == Key.F7;
			block |= key == Key.F8;
			block |= key == Key.F9;
			block |= key == Key.F10;
			block |= key == Key.F11;
			block |= key == Key.F12;
			block |= key == Key.PrintScreen;

			block |= key == Key.Escape && modifier.HasFlag(KeyModifier.Alt);
			block |= key == Key.Escape && modifier.HasFlag(KeyModifier.Ctrl);
			block |= key == Key.Space && modifier.HasFlag(KeyModifier.Alt);

			block |= !settings.AllowAltTab && key == Key.Tab && modifier.HasFlag(KeyModifier.Alt);
			block |= !settings.AllowEsc && key == Key.Escape && modifier == KeyModifier.None;
			block |= !settings.AllowF5 && key == Key.F5;

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
