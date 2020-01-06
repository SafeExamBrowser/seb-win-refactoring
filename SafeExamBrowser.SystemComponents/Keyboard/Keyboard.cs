/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard.Events;

namespace SafeExamBrowser.SystemComponents.Keyboard
{
	public class Keyboard : IKeyboard
	{
		private IList<KeyboardLayout> layouts;
		private ILogger logger;
		private CultureInfo originalLanguage;

		public event LayoutChangedEventHandler LayoutChanged;

		public Keyboard(ILogger logger)
		{
			this.layouts = new List<KeyboardLayout>();
			this.logger = logger;
		}

		public void ActivateLayout(Guid layoutId)
		{
			var layout = layouts.FirstOrDefault(l => l.Id == layoutId);

			if (layout != default(KeyboardLayout))
			{
				logger.Info($"Changing keyboard layout to {ToString(layout.CultureInfo)}.");
				InputLanguageManager.Current.CurrentInputLanguage = layout.CultureInfo;
			}
			else
			{
				logger.Error($"Could not find keyboard layout with id '{layoutId}'!");
			}
		}

		public void Initialize()
		{
			originalLanguage = InputLanguageManager.Current.CurrentInputLanguage;
			logger.Info($"Saved current keyboard layout {ToString(originalLanguage)}.");

			foreach (CultureInfo info in InputLanguageManager.Current.AvailableInputLanguages)
			{
				var layout = new KeyboardLayout
				{
					CultureCode = info.ThreeLetterISOLanguageName.ToUpper(),
					CultureInfo = info,
					IsCurrent = originalLanguage.Equals(info),
					Name = info.NativeName
				};

				layouts.Add(layout);
				logger.Info($"Detected keyboard layout {ToString(info)}.");
			}

			InputLanguageManager.Current.InputLanguageChanged += Current_InputLanguageChanged;
		}

		public IEnumerable<IKeyboardLayout> GetLayouts()
		{
			return layouts;
		}

		public void Terminate()
		{
			InputLanguageManager.Current.InputLanguageChanged -= Current_InputLanguageChanged;

			if (originalLanguage != null)
			{
				InputLanguageManager.Current.CurrentInputLanguage = originalLanguage;
				logger.Info($"Restored original keyboard layout {ToString(originalLanguage)}.");
			}
		}

		private void Current_InputLanguageChanged(object sender, InputLanguageEventArgs e)
		{
			var layout = layouts.First(l => l.CultureInfo.Equals(e.NewLanguage));

			logger.Info($"Detected keyboard layout change from {ToString(e.PreviousLanguage)} to {ToString(e.NewLanguage)}.");
			LayoutChanged?.Invoke(layout);
		}

		private string ToString(CultureInfo info)
		{
			return $"'{info.DisplayName}' ({info.ThreeLetterISOLanguageName.ToUpper()})";
		}
	}
}
