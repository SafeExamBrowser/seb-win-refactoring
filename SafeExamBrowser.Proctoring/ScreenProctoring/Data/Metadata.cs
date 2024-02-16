/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Data
{
	internal class Metadata
	{
		private readonly IApplicationMonitor applicationMonitor;
		private readonly IBrowserApplication browser;
		private readonly ILogger logger;

		internal string ApplicationInfo { get; private set; }
		internal string BrowserInfo { get; private set; }
		internal TimeSpan Elapsed { get; private set; }
		internal string TriggerInfo { get; private set; }
		internal string Urls { get; private set; }
		internal string WindowTitle { get; private set; }

		internal Metadata(IApplicationMonitor applicationMonitor, IBrowserApplication browser, TimeSpan elapsed, ILogger logger)
		{
			this.applicationMonitor = applicationMonitor;
			this.browser = browser;
			this.Elapsed = elapsed;
			this.logger = logger;
		}

		internal void Capture(IntervalTrigger interval = default, KeyboardTrigger keyboard = default, MouseTrigger mouse = default)
		{
			CaptureApplicationData();
			CaptureBrowserData();

			if (interval != default)
			{
				CaptureIntervalTrigger(interval);
			}
			else if (keyboard != default)
			{
				CaptureKeyboardTrigger(keyboard);
			}
			else if (mouse != default)
			{
				CaptureMouseTrigger(mouse);
			}

			// TODO: Can only log URLs when allowed by policy in browser configuration!
			logger.Debug($"Captured metadata: {ApplicationInfo} / {BrowserInfo} / {TriggerInfo} / {Urls} / {WindowTitle}.");
		}

		internal string ToJson()
		{
			var json = new JObject
			{
				[Header.Metadata.ApplicationInfo] = ApplicationInfo,
				[Header.Metadata.BrowserInfo] = BrowserInfo,
				[Header.Metadata.BrowserUrls] = Urls,
				[Header.Metadata.TriggerInfo] = TriggerInfo,
				[Header.Metadata.WindowTitle] = WindowTitle
			};

			return json.ToString(Formatting.None);
		}

		private void CaptureApplicationData()
		{
			if (applicationMonitor.TryGetActiveApplication(out var application))
			{
				ApplicationInfo = BuildApplicationInfo(application);
				WindowTitle = string.IsNullOrEmpty(application.Window.Title) ? "-" : application.Window.Title;
			}
			else
			{
				ApplicationInfo = "-";
				WindowTitle = "-";
			}
		}

		private void CaptureBrowserData()
		{
			var windows = browser.GetWindows();

			BrowserInfo = string.Join(", ", windows.Select(w => $"{(w.IsMainWindow ? "Main" : "Additional")} Window: {w.Title} ({w.Url})"));
			Urls = string.Join(", ", windows.Select(w => w.Url));
		}

		private void CaptureIntervalTrigger(IntervalTrigger interval)
		{
			TriggerInfo = $"Maximum interval of {interval.ConfigurationValue}ms has been reached ({interval.TimeElapsed}ms).";
		}

		private void CaptureKeyboardTrigger(KeyboardTrigger keyboard)
		{
			var flags = Enum.GetValues(typeof(KeyModifier)).OfType<KeyModifier>().Where(m => m != KeyModifier.None && keyboard.Modifier.HasFlag(m));
			var modifiers = flags.Any() ? string.Join(" + ", flags) + " + " : string.Empty;

			TriggerInfo = $"'{modifiers}{keyboard.Key}' has been {keyboard.State.ToString().ToLower()}.";
		}

		private void CaptureMouseTrigger(MouseTrigger mouse)
		{
			if (mouse.Info.IsTouch)
			{
				TriggerInfo = $"Tap as {mouse.Button} mouse button has been {mouse.State.ToString().ToLower()} at ({mouse.Info.X}/{mouse.Info.Y}).";
			}
			else
			{
				TriggerInfo = $"{mouse.Button} mouse button has been {mouse.State.ToString().ToLower()} at ({mouse.Info.X}/{mouse.Info.Y}).";
			}
		}

		private string BuildApplicationInfo(ActiveApplication application)
		{
			var info = new StringBuilder();

			info.Append(application.Process.Name);

			if (application.Process.OriginalName != default)
			{
				info.Append($" ({application.Process.OriginalName}{(application.Process.Signature == default ? ")" : "")}");
			}

			if (application.Process.Signature != default)
			{
				info.Append($"{(application.Process.OriginalName == default ? "(" : ", ")}{application.Process.Signature})");
			}

			return info.ToString();
		}
	}
}
