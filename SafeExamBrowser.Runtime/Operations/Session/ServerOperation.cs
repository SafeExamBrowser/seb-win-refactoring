/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class ServerOperation : ConfigurationBaseOperation
	{
		private readonly IFileSystem fileSystem;
		private readonly IServerProxy server;

		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			Dependencies dependencies,
			IFileSystem fileSystem,
			IConfigurationRepository repository,
			IServerProxy server,
			IUserInterfaceFactory uiFactory) : base(dependencies, repository, uiFactory)
		{
			this.fileSystem = fileSystem;
			this.server = server;
		}

		public override OperationResult Perform()
		{
			var result = OperationResult.Success;

			if (Context.Next.Settings.SessionMode == SessionMode.Server)
			{
				var browserExamKey = default(string);
				var exam = default(Exam);
				var exams = default(IEnumerable<Exam>);
				var uri = default(Uri);

				Logger.Info("Initializing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServer);

				server.Initialize(Context.Next.Settings.Server);

				var (abort, fallback, success) = TryPerformWithFallback(() => server.Connect());

				if (success)
				{
					(abort, fallback, success) = TryPerformWithFallback(() => server.GetAvailableExams(Context.Next.Settings.Server.ExamId), out exams);
				}

				if (success)
				{
					success = TrySelectExam(exams, out exam);
				}

				if (success)
				{
					(abort, fallback, success) = TryPerformWithFallback(() => server.GetConfigurationFor(exam), out uri);
				}

				if (success)
				{
					result = TryLoadServerSettings(exam, uri);
				}

				if (success && result == OperationResult.Success)
				{
					(abort, fallback, success) = TryPerformWithFallback(() => server.SendSelectedExam(exam), out browserExamKey);
				}

				if (browserExamKey != default)
				{
					Context.Next.Settings.Browser.CustomBrowserExamKey = browserExamKey;
				}

				if (abort)
				{
					result = OperationResult.Aborted;
					Logger.Info("The user aborted the server operation.");
				}

				if (fallback)
				{
					Context.Next.Settings.SessionMode = SessionMode.Normal;
					result = OperationResult.Success;
					Logger.Info("The user chose to fallback and start a normal session.");
				}

				if (result == OperationResult.Success)
				{
					Logger.Info("Successfully initialized server.");
				}
				else if (result == OperationResult.Failed)
				{
					Logger.Error("Failed to initialize server!");
				}
			}

			return result;
		}

		public override OperationResult Repeat()
		{
			var result = OperationResult.Success;

			if (Context.Current.Settings.SessionMode == SessionMode.Server && Context.Next.Settings.SessionMode == SessionMode.Server)
			{
				result = Revert();

				if (result == OperationResult.Success)
				{
					result = Perform();
				}
				else
				{
					Logger.Error($"Cannot start new server session due to failed finalization of current server session! Terminating...");
				}
			}
			else if (Context.Current.Settings.SessionMode == SessionMode.Server)
			{
				result = Revert();
			}
			else if (Context.Next.Settings.SessionMode == SessionMode.Server)
			{
				result = Perform();
			}

			return result;
		}

		public override OperationResult Revert()
		{
			var result = OperationResult.Success;

			if (Context.Current?.Settings.SessionMode == SessionMode.Server || Context.Next?.Settings.SessionMode == SessionMode.Server)
			{
				Logger.Info("Finalizing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServer);

				var disconnect = server.Disconnect();

				if (disconnect.Success)
				{
					result = OperationResult.Success;
					Logger.Info("Successfully finalized server.");
				}
				else
				{
					result = OperationResult.Failed;
					Logger.Error("Failed to finalize server!");
				}
			}

			return result;
		}

		private OperationResult TryLoadServerSettings(Exam exam, Uri uri)
		{
			var info = server.GetConnectionInfo();
			var result = OperationResult.Failed;
			var status = TryLoadSettings(uri, UriSource.Server, out _, out var settings);

			fileSystem.Delete(uri.LocalPath);

			if (status == LoadStatus.Success)
			{
				var browserSettings = Context.Next.Settings.Browser;
				var serverSettings = Context.Next.Settings.Server;

				Context.Next.AppConfig.ServerApi = info.Api;
				Context.Next.AppConfig.ServerConnectionToken = info.ConnectionToken;
				Context.Next.AppConfig.ServerExamId = exam.Id;
				Context.Next.AppConfig.ServerOauth2Token = info.Oauth2Token;

				Context.Next.Settings = settings;
				Context.Next.Settings.Browser.StartUrl = exam.Url;
				Context.Next.Settings.Browser.StartUrlQuery = browserSettings.StartUrlQuery;
				Context.Next.Settings.Server = serverSettings;
				Context.Next.Settings.Server.Invigilation = settings.Server.Invigilation;
				Context.Next.Settings.SessionMode = SessionMode.Server;

				result = OperationResult.Success;
			}

			return result;
		}

		private (bool abort, bool fallback, bool success) TryPerformWithFallback(Func<ServerResponse> request)
		{
			var abort = false;
			var fallback = false;
			var success = false;

			while (!success)
			{
				var response = request();

				success = response.Success;

				if (!success && !Retry(response.Message, out abort, out fallback))
				{
					break;
				}
			}

			return (abort, fallback, success);
		}

		private (bool abort, bool fallback, bool success) TryPerformWithFallback<T>(Func<ServerResponse<T>> request, out T value)
		{
			var abort = false;
			var fallback = false;
			var success = false;

			value = default;

			while (!success)
			{
				var response = request();

				success = response.Success;
				value = response.Value;

				if (!success && !Retry(response.Message, out abort, out fallback))
				{
					break;
				}
			}

			return (abort, fallback, success);
		}

		private bool Retry(string message, out bool abort, out bool fallback)
		{
			AskForServerFailureAction(message, out abort, out fallback, out var retry);

			if (retry)
			{
				Logger.Debug("The user chose to retry the current server request.");
			}

			return retry;
		}

		private void AskForServerFailureAction(string message, out bool abort, out bool fallback, out bool retry)
		{
			var showFallback = Context.Next.Settings.Server.PerformFallback;

			if (ClientBridge.IsRequired())
			{
				ClientBridge.TryAskForServerFailureAction(message, showFallback, out abort, out fallback, out retry);
			}
			else
			{
				var dialog = uiFactory.CreateServerFailureDialog(message, showFallback);
				var result = dialog.Show(RuntimeWindow);

				abort = result.Abort;
				fallback = result.Fallback;
				retry = result.Retry;
			}
		}

		private bool TrySelectExam(IEnumerable<Exam> exams, out Exam exam)
		{
			var success = true;

			if (string.IsNullOrWhiteSpace(Context.Next.Settings.Server.ExamId))
			{
				success = TryAskForExamSelection(exams, out exam);
			}
			else
			{
				exam = exams.First();
				Logger.Info("Automatically selected exam as defined in configuration.");
			}

			return success;
		}

		private bool TryAskForExamSelection(IEnumerable<Exam> exams, out Exam exam)
		{
			var success = false;

			if (ClientBridge.IsRequired())
			{
				success = ClientBridge.TryAskForExamSelection(exams, out exam);
			}
			else
			{
				var dialog = uiFactory.CreateExamSelectionDialog(exams);
				var result = dialog.Show(RuntimeWindow);

				exam = result.SelectedExam;
				success = result.Success;
			}

			return success;
		}
	}
}
