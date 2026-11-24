/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard.Events;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.SystemComponents.Keyboard
{
	/// <summary>
	/// IMPORTANT: All members must be used on the same (STA) thread which performed the instantiation and initialization, see <see cref="ITextService"/>.
	/// </summary>
	public class Keyboard : IKeyboard
	{
		private readonly IList<KeyboardLayout> layouts;
		private readonly ILogger logger;

		private KeyboardLayout original;
		private readonly ITextService textService;

		public event LayoutChangedEventHandler LayoutChanged;

		public Keyboard(ILogger logger, ITextService textService)
		{
			this.layouts = new List<KeyboardLayout>();
			this.logger = logger;
			this.textService = textService;
		}

		public void ActivateLayout(Guid layoutId)
		{
			var layout = layouts.First(l => l.Id == layoutId);

			logger.Info($"Changing keyboard layout to {layout}...");

			try
			{
				textService.ActivateProfile(layout.Profile);
				SetCurrent(layout);
			}
			catch (Exception e)
			{
				logger.Error($"Failed to change keyboard layout to {layout}!", e);
			}
		}

		public void Initialize()
		{
			InitializeLayouts();
			InitializeOriginal();

			textService.ProfileActivated += TextService_ProfileActivated;
			textService.StartMonitoring();
		}

		public IEnumerable<IKeyboardLayout> GetLayouts()
		{
			return new List<KeyboardLayout>(layouts.OrderBy(l => l.CultureName));
		}

		public void Terminate()
		{
			textService.ProfileActivated -= TextService_ProfileActivated;
			textService.StopMonitoring();

			if (original != null)
			{
				try
				{
					textService.ActivateProfile(original.Profile);
					logger.Info($"Restored original keyboard layout {original}.");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to restore original keyboard layout {original}!", e);
				}
			}

			textService.Dispose();
		}

		private void InitializeLayouts()
		{
			foreach (var profile in textService.GetProfiles())
			{
				if (profile.IsEnabled && profile.IsKeyboard && !profile.IsSubstituted)
				{
					var layout = CreateLayout(profile);

					layouts.Add(layout);
					logger.Info($"Detected keyboard layout {layout}.");
				}
				else
				{
					logger.Debug($"Ignoring input profile '{profile}' (enabled: {profile.IsEnabled}, keyboard: {profile.IsKeyboard}, substituted: {profile.IsSubstituted}).");
				}
			}
		}

		private void InitializeOriginal()
		{
			if (textService.TryGetActiveProfile(out var active))
			{
				original = layouts.FirstOrDefault(l => l.Profile.Equals(active));
			}

			if (original == default(KeyboardLayout))
			{
				original = layouts.FirstOrDefault(l => l.Profile.IsActive);
			}

			if (original != default(KeyboardLayout))
			{
				original.IsCurrent = true;
				logger.Info($"Saved current keyboard layout {original}.");
			}
			else
			{
				logger.Warn("Failed to determine the current keyboard layout!");
			}
		}

		private KeyboardLayout CreateLayout(IInputProfile profile)
		{
			var layout = new KeyboardLayout { Profile = profile };

			if (textService.TryGetCulture(profile.LanguageId, out var culture))
			{
				layout.CultureCode = culture.ThreeLetterISOLanguageName.ToUpper();
				layout.CultureName = culture.NativeName;
			}
			else
			{
				layout.CultureCode = $"{profile.LanguageId:X4}";
				layout.CultureName = $"0x{profile.LanguageId:X4}";
				logger.Warn($"Failed to determine culture of input profile '{profile}'!");
			}

			if (profile.IsTextService)
			{
				try
				{
					layout.LayoutName = textService.GetDescription(profile);
				}
				catch (Exception e)
				{
					layout.LayoutName = "-";
					logger.Error($"Failed to retrieve description of input profile '{profile}'!", e);
				}
			}
			else
			{
				layout.LayoutName = textService.GetKeyboardLayoutName(profile.Hkl);
			}

			return layout;
		}

		private void SetCurrent(KeyboardLayout layout)
		{
			foreach (var l in layouts)
			{
				l.IsCurrent = l == layout;
			}

			LayoutChanged?.Invoke(layout);
		}

		private void TextService_ProfileActivated(IInputProfile profile)
		{
			var layout = layouts.FirstOrDefault(l => l.Profile.Equals(profile));

			if (layout != default(KeyboardLayout))
			{
				logger.Info($"Detected keyboard layout change to {layout}.");
				SetCurrent(layout);
			}
			else
			{
				logger.Warn($"Detected activation of unknown input profile '{profile}'!");
			}
		}
	}
}
