/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard.Events;

namespace SafeExamBrowser.SystemComponents.Keyboard
{
	public class Keyboard : IKeyboard
	{
		private readonly IList<KeyboardLayout> layouts;
		private readonly ILogger logger;

		private InputLanguage originalLanguage;

		public event LayoutChangedEventHandler LayoutChanged;

		public Keyboard(ILogger logger)
		{
			this.layouts = new List<KeyboardLayout>();
			this.logger = logger;
		}

		public void ActivateLayout(Guid layoutId)
		{
			var layout = layouts.First(l => l.Id == layoutId);

			logger.Info($"Changing keyboard layout to {layout}...");
			InputLanguage.CurrentInputLanguage = layout.InputLanguage;

			layout.IsCurrent = true;
			LayoutChanged?.Invoke(layout);
		}

		public void Initialize()
		{
			originalLanguage = InputLanguage.CurrentInputLanguage;
			logger.Info($"Saved current keyboard layout {ToString(originalLanguage)}.");

			foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
			{
				var layout = new KeyboardLayout
				{
					CultureCode = language.Culture.ThreeLetterISOLanguageName.ToUpper(),
					CultureName = language.Culture.NativeName,
					InputLanguage = language,
					IsCurrent = originalLanguage.Equals(language),
					LayoutName = language.LayoutName
				};

				layouts.Add(layout);
				logger.Info($"Detected keyboard layout {layout}.");
			}

			InputLanguageManager.Current.InputLanguageChanged += InputLanguageManager_InputLanguageChanged;
		}

		public IEnumerable<IKeyboardLayout> GetLayouts()
		{
			return new List<KeyboardLayout>(layouts.OrderBy(l => l.CultureName));
		}

		public void Terminate()
		{
			InputLanguageManager.Current.InputLanguageChanged -= InputLanguageManager_InputLanguageChanged;

			if (originalLanguage != null)
			{
				InputLanguage.CurrentInputLanguage = originalLanguage;
				logger.Info($"Restored original keyboard layout {ToString(originalLanguage)}.");
			}
		}

		private void InputLanguageManager_InputLanguageChanged(object sender, InputLanguageEventArgs e)
		{
			var layout = layouts.First(l => l.InputLanguage.Culture.Equals(e.NewLanguage));

			logger.Info($"Detected keyboard layout change from {ToString(e.PreviousLanguage)} to {ToString(e.NewLanguage)}.");
			layout.IsCurrent = true;
			LayoutChanged?.Invoke(layout);
		}

		private string ToString(InputLanguage language)
		{
			return $"'{language.Culture.NativeName}' [{language.Culture.ThreeLetterISOLanguageName.ToUpper()}, {language.LayoutName}]";
		}

		private string ToString(CultureInfo culture)
		{
			return $"'{culture.NativeName}' [{culture.ThreeLetterISOLanguageName.ToUpper()}]";
		}
	}
}
