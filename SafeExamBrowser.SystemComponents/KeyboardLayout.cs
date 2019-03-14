/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.SystemComponents
{
	public class KeyboardLayout : ISystemComponent<ISystemKeyboardLayoutControl>
	{
		private const int TWO_SECONDS = 2000;

		private KeyboardLayoutDefinition currentLayout;
		private IList<KeyboardLayoutDefinition> layouts;
		private ILogger logger;
		private CultureInfo originalLanguage;
		private IList<ISystemKeyboardLayoutControl> controls;
		private IText text;

		public KeyboardLayout(ILogger logger, IText text)
		{
			this.controls = new List<ISystemKeyboardLayoutControl>();
			this.layouts = new List<KeyboardLayoutDefinition>();
			this.logger = logger;
			this.text = text;
		}

		public void Initialize()
		{
			originalLanguage = InputLanguageManager.Current.CurrentInputLanguage;
			logger.Info($"Saved current keyboard layout {ToString(originalLanguage)}.");

			foreach (CultureInfo info in InputLanguageManager.Current.AvailableInputLanguages)
			{
				var layout = new KeyboardLayoutDefinition
				{
					CultureCode = info.ThreeLetterISOLanguageName.ToUpper(),
					CultureInfo = info,
					Name = info.NativeName
				};

				if (originalLanguage.Equals(info))
				{
					currentLayout = layout;
				}

				layouts.Add(layout);
				logger.Info($"Detected keyboard layout {ToString(info)}.");
			}

			InputLanguageManager.Current.InputLanguageChanged += Current_InputLanguageChanged;
		}

		public void Register(ISystemKeyboardLayoutControl control)
		{
			foreach (var layout in layouts)
			{
				control.Add(layout);
			}

			control.LayoutSelected += Control_LayoutSelected;
			control.SetCurrent(currentLayout);
			control.SetInformation(GetInfoTextFor(currentLayout));

			controls.Add(control);
		}

		public void Terminate()
		{
			InputLanguageManager.Current.InputLanguageChanged -= Current_InputLanguageChanged;

			if (originalLanguage != null)
			{
				InputLanguageManager.Current.CurrentInputLanguage = originalLanguage;
				logger.Info($"Restored original keyboard layout {ToString(originalLanguage)}.");
			}

			foreach (var control in controls)
			{
				control.Close();
			}
		}

		private void Control_LayoutSelected(Guid id)
		{
			var layout = layouts.First(l => l.Id == id);

			logger.Info($"Changing keyboard layout to {ToString(layout.CultureInfo)}.");
			InputLanguageManager.Current.CurrentInputLanguage = layout.CultureInfo;
		}

		private void Current_InputLanguageChanged(object sender, InputLanguageEventArgs e)
		{
			var newLayout = layouts.First(l => l.CultureInfo.Equals(e.NewLanguage));

			logger.Info($"Detected keyboard layout change from {ToString(e.PreviousLanguage)} to {ToString(e.NewLanguage)}.");
			currentLayout = newLayout;

			foreach (var control in controls)
			{
				control.SetCurrent(newLayout);
				control.SetInformation(GetInfoTextFor(newLayout));
			}
		}

		private string GetInfoTextFor(KeyboardLayoutDefinition layout)
		{
			return text.Get(TextKey.SystemControl_KeyboardLayoutTooltip).Replace("%%LAYOUT%%", layout.CultureInfo.NativeName);
		}

		private string ToString(CultureInfo info)
		{
			return $"'{info.DisplayName}' ({info.ThreeLetterISOLanguageName.ToUpper()})";
		}
	}
}
