/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal abstract class WindowResponsibility : IResponsibility<WindowTask>
	{
		protected BrowserWindowContext Context { get; }

		protected IBrowserControl Control => Context.Control;
		protected ILogger Logger => Context.Logger;
		protected IMessageBox MessageBox => Context.MessageBox;
		protected BrowserSettings Settings => Context.Settings;
		protected IText Text => Context.Text;
		protected IBrowserWindow Window => Context.Window;
		protected WindowSettings WindowSettings => Context.IsMainWindow ? Settings.MainWindow : Settings.AdditionalWindow;

		internal WindowResponsibility(BrowserWindowContext context)
		{
			Context = context;
		}

		public abstract void Assume(WindowTask task);

		protected void HomeNavigationRequested()
		{
			if (Context.IsMainWindow && (Settings.UseStartUrlAsHomeUrl || !string.IsNullOrWhiteSpace(Settings.HomeUrl)))
			{
				var navigate = false;
				var url = Settings.UseStartUrlAsHomeUrl ? Settings.StartUrl : Settings.HomeUrl;

				if (Settings.HomeNavigationRequiresPassword && !string.IsNullOrWhiteSpace(Settings.HomePasswordHash))
				{
					var message = Context.Text.Get(TextKey.PasswordDialog_BrowserHomePasswordRequired);
					var title = !string.IsNullOrWhiteSpace(Settings.HomeNavigationMessage) ? Settings.HomeNavigationMessage : Context.Text.Get(TextKey.PasswordDialog_BrowserHomePasswordRequiredTitle);
					var dialog = Context.UserInterfaceFactory.CreatePasswordDialog(message, title);
					var result = dialog.Show(Window);

					if (result.Success)
					{
						var passwordHash = Context.HashAlgorithm.GenerateHashFor(result.Password);

						if (Settings.HomePasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
						{
							navigate = true;
						}
						else
						{
							Context.MessageBox.Show(TextKey.MessageBox_InvalidHomePassword, TextKey.MessageBox_InvalidHomePasswordTitle, icon: MessageBoxIcon.Warning, parent: Window);
						}
					}
				}
				else
				{
					var message = Context.Text.Get(TextKey.MessageBox_BrowserHomeQuestion);
					var title = !string.IsNullOrWhiteSpace(Settings.HomeNavigationMessage) ? Settings.HomeNavigationMessage : Context.Text.Get(TextKey.MessageBox_BrowserHomeQuestionTitle);
					var result = Context.MessageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, Window);

					navigate = result == MessageBoxResult.Yes;
				}

				if (navigate)
				{
					Control.NavigateTo(url);
				}
			}
		}

		protected void ReloadRequested()
		{
			Logger.Debug("A reload of the current page has been requested...");

			if (RequestPageReload())
			{
				Control.Reload();
			}
		}

		protected bool RequestPageReload()
		{
			var allow = false;

			if (WindowSettings.AllowReloading && WindowSettings.ShowReloadWarning)
			{
				var result = Context.MessageBox.Show(TextKey.MessageBox_PageReloadConfirmation, TextKey.MessageBox_PageReloadConfirmationTitle, MessageBoxAction.YesNo, MessageBoxIcon.Question, Window);

				if (result == MessageBoxResult.Yes)
				{
					allow = true;
					Logger.Debug("The page reload has been granted by the user.");
				}
				else
				{
					Logger.Debug("The page reload has been aborted by the user.");
				}
			}
			else if (WindowSettings.AllowReloading)
			{
				allow = true;
				Logger.Debug("The page reload has been automatically granted.");
			}
			else
			{
				Logger.Debug("The page reload has been blocked, as the user is not allowed to reload web pages.");
			}

			return allow;
		}

		protected void ShowDownUploadNotAllowedMessage(bool isDownload = true)
		{
			var message = isDownload ? TextKey.MessageBox_DownloadNotAllowed : TextKey.MessageBox_UploadNotAllowed;
			var title = isDownload ? TextKey.MessageBox_DownloadNotAllowedTitle : TextKey.MessageBox_UploadNotAllowedTitle;

			Context.MessageBox.Show(message, title, icon: MessageBoxIcon.Warning, parent: Window);
		}
	}
}
