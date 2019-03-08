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
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.SystemComponents
{
	public class KeyboardLayout : ISystemComponent<ISystemKeyboardLayoutControl>
	{
		private IList<KeyboardLayoutDefinition> layouts = new List<KeyboardLayoutDefinition>();
		private ILogger logger;
		private InputLanguage originalLanguage;
		private IList<ISystemKeyboardLayoutControl> controls;
		private IText text;

		public KeyboardLayout(ILogger logger, IText text)
		{
			this.controls = new List<ISystemKeyboardLayoutControl>();
			this.logger = logger;
			this.text = text;
		}

		public void Initialize()
		{
			originalLanguage = InputLanguage.CurrentInputLanguage;
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

				layouts.Add(layout);
				logger.Info($"Detected keyboard layout {ToString(language)}.");
			}
		}

		public void Register(ISystemKeyboardLayoutControl control)
		{
			control.LayoutSelected += Control_LayoutSelected;
			control.SetTooltip(text.Get(TextKey.SystemControl_KeyboardLayoutTooltip));

			foreach (var layout in layouts)
			{
				control.Add(layout);
			}

			controls.Add(control);
		}

		public void Terminate()
		{
			if (originalLanguage != null)
			{
				InputLanguage.CurrentInputLanguage = originalLanguage;
				logger.Info($"Restored original keyboard layout {ToString(originalLanguage)}.");
			}

			foreach (var control in controls)
			{
				control.Close();
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
