/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.SystemComponents
{
	public class KeyboardLayout : ISystemComponent<ISystemKeyboardLayoutControl>
	{
		private IList<KeyboardLayoutDefinition> layouts = new List<KeyboardLayoutDefinition>();
		private ILogger logger;
		private InputLanguage originalLanguage;
		private ISystemKeyboardLayoutControl control;
		private IText text;

		public KeyboardLayout(ILogger logger, IText text)
		{
			this.logger = logger;
			this.text = text;
		}

		public void Initialize(ISystemKeyboardLayoutControl control)
		{
			this.control = control;

			originalLanguage = InputLanguage.CurrentInputLanguage;
			control.LayoutSelected += Control_LayoutSelected;
			control.SetTooltip(text.Get(TextKey.SystemControl_KeyboardLayoutTooltip));

			logger.Info($"Saved current keyboard layout {ToString(originalLanguage)}.");

			foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
			{
				var layout = new KeyboardLayoutDefinition
				{
					CultureCode = language.Culture.ThreeLetterISOLanguageName.ToUpper(),
					IsCurrent = originalLanguage.Equals(language),
					Language = language,
					Name = language.LayoutName
				};

				control.Add(layout);
				layouts.Add(layout);

				logger.Info($"Added keyboard layout {ToString(language)} to system control.");
			}
		}

		public void Terminate()
		{
			control?.Close();

			if (originalLanguage != null)
			{
				InputLanguage.CurrentInputLanguage = originalLanguage;
				logger.Info($"Restored original keyboard layout {ToString(originalLanguage)}.");
			}
		}

		private void Control_LayoutSelected(IKeyboardLayout layout)
		{
			var language = layouts.First(l => l.Id == layout.Id).Language;

			InputLanguage.CurrentInputLanguage = language;

			foreach (var l in layouts)
			{
				l.IsCurrent = l.Id == layout.Id;
			}

			logger.Info($"Changed keyboard layout to {ToString(language)}.");
		}

		private string ToString(InputLanguage language)
		{
			return $"'{language.LayoutName}' ({language.Culture.ThreeLetterISOLanguageName.ToUpper()})";
		}
	}
}
