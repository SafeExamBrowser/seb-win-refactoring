/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using Syroot.Windows.IO;
using MessageBoxIcon = SafeExamBrowser.UserInterface.Contracts.MessageBox.MessageBoxIcon;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class DialogResponsibility : WindowResponsibility
	{
		private readonly DialogHandler dialogHandler;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly JavaScriptDialogHandler javaScriptDialogHandler;

		public DialogResponsibility(
			BrowserWindowContext context,
			DialogHandler dialogHandler,
			IFileSystemDialog fileSystemDialog,
			JavaScriptDialogHandler javaScriptDialogHandler) : base(context)
		{
			this.dialogHandler = dialogHandler;
			this.fileSystemDialog = fileSystemDialog;
			this.javaScriptDialogHandler = javaScriptDialogHandler;
		}

		public override void Assume(WindowTask task)
		{
			if (task == WindowTask.RegisterEvents)
			{
				RegisterEvents();
			}
		}

		private void RegisterEvents()
		{
			dialogHandler.DialogRequested += DialogHandler_DialogRequested;
			javaScriptDialogHandler.DialogRequested += JavaScriptDialogHandler_DialogRequested;
		}

		private void DialogHandler_DialogRequested(DialogRequestedEventArgs args)
		{
			var isDownload = args.Operation == FileSystemOperation.Save;
			var isUpload = args.Operation == FileSystemOperation.Open;
			var isAllowed = (isDownload && Settings.AllowDownloads) || (isUpload && Settings.AllowUploads);
			var initialPath = default(string);

			if (isDownload)
			{
				initialPath = args.InitialPath;
			}
			else if (string.IsNullOrEmpty(Settings.DownAndUploadDirectory))
			{
				initialPath = KnownFolders.Downloads.ExpandedPath;
			}
			else
			{
				initialPath = Environment.ExpandEnvironmentVariables(Settings.DownAndUploadDirectory);
			}

			if (isAllowed)
			{
				var result = fileSystemDialog.Show(
					args.Element,
					args.Operation,
					initialPath,
					title: args.Title,
					parent: Window,
					restrictNavigation: !Settings.AllowCustomDownAndUploadLocation,
					showElementPath: Settings.ShowFileSystemElementPath);

				if (result.Success)
				{
					args.FullPath = result.FullPath;
					args.Success = result.Success;
					Logger.Debug($"User selected path '{result.FullPath}' when asked to {args.Operation}->{args.Element}.");
				}
				else
				{
					Logger.Debug($"User aborted file system dialog to {args.Operation}->{args.Element}.");
				}
			}
			else
			{
				Logger.Info($"Blocked file system dialog to {args.Operation}->{args.Element}, as {(isDownload ? "downloading" : "uploading")} is not allowed.");
				ShowDownUploadNotAllowedMessage(isDownload);
			}
		}

		private void JavaScriptDialogHandler_DialogRequested(JavaScriptDialogRequestedEventArgs args)
		{
			Logger.Debug($"A JavaScript dialog of type '{args.Type}' has been requested...");

			if (args.Type == JavaScriptDialogType.LeavePage)
			{
				args.Success = RequestPageLeave();
			}
			else
			{
				args.Success = RequestPageReload();
			}
		}

		private bool RequestPageLeave()
		{
			var allow = false;
			var result = MessageBox.Show(TextKey.MessageBox_PageLeaveConfirmation, TextKey.MessageBox_PageLeaveConfirmationTitle, MessageBoxAction.YesNo, MessageBoxIcon.Question, Window);

			if (result == MessageBoxResult.Yes)
			{
				allow = true;
				Logger.Debug("The page leave has been granted by the user.");
			}
			else
			{
				Logger.Debug("The page leave has been aborted by the user.");
			}

			return allow;
		}
	}
}
