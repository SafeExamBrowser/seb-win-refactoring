/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class RequestResponsibility : WindowResponsibility
	{
		private readonly RequestFilter requestFilter;
		private readonly RequestHandler requestHandler;
		private readonly ResourceHandler resourceHandler;

		internal event ResetRequestedEventHandler ResetRequested;
		internal event TerminationRequestedEventHandler TerminationRequested;
		internal event TitleChangedEventHandler TitleChanged;
		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

		public RequestResponsibility(
			BrowserWindowContext context,
			RequestFilter requestFilter,
			RequestHandler requestHandler,
			ResourceHandler resourceHandler) : base(context)
		{
			this.requestFilter = requestFilter;
			this.requestHandler = requestHandler;
			this.resourceHandler = resourceHandler;
		}

		public override void Assume(WindowTask task)
		{
			switch (task)
			{
				case WindowTask.InitializeRequestFilter:
					InitializeRequestFilter();
					break;
				case WindowTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void InitializeRequestFilter()
		{
			if (Settings.Filter.ProcessContentRequests || Settings.Filter.ProcessMainRequests)
			{
				var factory = new RuleFactory();

				foreach (var settings in Settings.Filter.Rules)
				{
					var rule = factory.CreateRule(settings.Type);

					rule.Initialize(settings);
					requestFilter.Load(rule);
				}

				Logger.Debug($"Initialized request filter with {Settings.Filter.Rules.Count} rule(s).");

				if (requestFilter.Process(new Request { Url = Settings.StartUrl }) != FilterResult.Allow)
				{
					var rule = factory.CreateRule(FilterRuleType.Simplified);

					rule.Initialize(new FilterRuleSettings { Expression = Settings.StartUrl, Result = FilterResult.Allow });
					requestFilter.Load(rule);

					Logger.Debug($"Automatically created filter rule to allow start URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{Settings.StartUrl}'" : "")}.");
				}
			}
		}

		private void RegisterEvents()
		{
			requestHandler.QuitUrlVisited += RequestHandler_QuitUrlVisited;
			requestHandler.RequestBlocked += RequestHandler_RequestBlocked;
			resourceHandler.UserIdentifierDetected += (id) => UserIdentifierDetected?.Invoke(id);
		}

		private void RequestHandler_QuitUrlVisited(string url)
		{
			Task.Run(() =>
			{
				if (Settings.ResetOnQuitUrl)
				{
					Logger.Info("Forwarding request to reset browser...");
					ResetRequested?.Invoke();
				}
				else
				{
					if (Settings.ConfirmQuitUrl)
					{
						var message = Text.Get(TextKey.MessageBox_BrowserQuitUrlConfirmation);
						var title = Text.Get(TextKey.MessageBox_BrowserQuitUrlConfirmationTitle);
						var result = MessageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, Window);
						var terminate = result == MessageBoxResult.Yes;

						if (terminate)
						{
							Logger.Info($"User confirmed termination via quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}, forwarding request...");
							TerminationRequested?.Invoke();
						}
						else
						{
							Logger.Info($"User aborted termination via quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}.");
						}
					}
					else
					{
						Logger.Info($"Automatically requesting termination due to quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}...");
						TerminationRequested?.Invoke();
					}
				}
			});
		}

		private void RequestHandler_RequestBlocked(string url)
		{
			Task.Run(() =>
			{
				var message = Text.Get(TextKey.MessageBox_BrowserNavigationBlocked).Replace("%%URL%%", WindowSettings.UrlPolicy.CanLogError() ? url : "");
				var title = Text.Get(TextKey.MessageBox_BrowserNavigationBlockedTitle);

				Window.UpdateTitle($"*** {title} ***");
				TitleChanged?.Invoke($"*** {title} ***");

				MessageBox.Show(message, title, parent: Window);
			});
		}
	}
}
