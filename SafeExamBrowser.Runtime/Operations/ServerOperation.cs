/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServerOperation : ConfigurationBaseOperation
	{
		private readonly IFileSystem fileSystem;
		private readonly ILogger logger;
		private readonly IServerProxy server;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			IFileSystem fileSystem,
			ILogger logger,
			SessionContext context,
			IServerProxy server) : base(commandLineArgs, configuration, context)
		{
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.server = server;
		}

		public override OperationResult Perform()
		{
			var result = OperationResult.Success;

			if (Context.Next.Settings.SessionMode == SessionMode.Server)
			{
				logger.Info("Initializing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServer);

				server.Initialize(Context.Next.Settings.Server);

				var (abort, fallback, success) = TryPerformWithFallback(() => server.Connect());

				if (success)
				{
					(abort, fallback, success) = TryPerformWithFallback(() => server.GetAvailableExams(Context.Next.Settings.Server.ExamId), out var exams);

					if (success)
					{
						success = TrySelectExam(exams, out var exam);

						if (success)
						{
							(abort, fallback, success) = TryPerformWithFallback(() => server.GetConfigurationFor(exam), out var uri);

							if (success)
							{
								result = TryLoadServerSettings(exam, uri);
							}
						}
					}
				}

				if (abort)
				{
					result = OperationResult.Aborted;
					logger.Info("The user aborted the server operation.");
				}

				if (fallback)
				{
					Context.Next.Settings.SessionMode = SessionMode.Normal;
					result = OperationResult.Success;
					logger.Info("The user chose to fallback and start a normal session.");
				}
			}

			return result;
		}

		public override OperationResult Repeat()
		{
			if (Context.Current.Settings.SessionMode == SessionMode.Server)
			{
				if (Context.Next.Settings.SessionMode == SessionMode.Server)
				{
					ShowReconfigurationError();

					return OperationResult.Aborted;
				}
				else
				{
					return Revert();
				}
			}
			else if (Context.Next.Settings.SessionMode == SessionMode.Server)
			{
				return Perform();
			}

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			var result = OperationResult.Success;

			if (Context.Current?.Settings.SessionMode == SessionMode.Server)
			{
				logger.Info("Finalizing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServer);

				var disconnect = server.Disconnect();

				if (disconnect.Success)
				{
					result = OperationResult.Success;
				}
				else
				{
					result = OperationResult.Failed;
				}
			}

			return result;
		}

		protected override void InvokeActionRequired(ActionRequiredEventArgs args)
		{
			ActionRequired?.Invoke(args);
		}

		private OperationResult TryLoadServerSettings(Exam exam, Uri uri)
		{
			var info = server.GetConnectionInfo();
			var result = OperationResult.Failed;
			var status = TryLoadSettings(uri, UriSource.Server, out _, out var settings);

			fileSystem.Delete(uri.LocalPath);

			if (status == LoadStatus.Success)
			{
				var serverSettings = Context.Next.Settings.Server;

				Context.Next.AppConfig.ServerApi = info.Api;
				Context.Next.AppConfig.ServerConnectionToken = info.ConnectionToken;
				Context.Next.AppConfig.ServerExamId = exam.Id;
				Context.Next.AppConfig.ServerOauth2Token = info.Oauth2Token;

				Context.Next.Settings = settings;
				Context.Next.Settings.Browser.StartUrl = exam.Url;
				Context.Next.Settings.Server = serverSettings;
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
			var args = new ServerFailureEventArgs(message, Context.Next.Settings.Server.PerformFallback);

			ActionRequired?.Invoke(args);

			abort = args.Abort;
			fallback = args.Fallback;

			return args.Retry;
		}

		private bool TrySelectExam(IEnumerable<Exam> exams, out Exam exam)
		{
			var success = true;

			if (string.IsNullOrWhiteSpace(Context.Next.Settings.Server.ExamId))
			{
				var args = new ExamSelectionEventArgs(exams);

				ActionRequired?.Invoke(args);

				exam = args.SelectedExam;
				success = args.Success;
			}
			else
			{
				exam = exams.First();
				logger.Info("Automatically selected exam as defined in configuration.");
			}

			return success;
		}

		private void ShowReconfigurationError()
		{
			var args = new MessageEventArgs
			{
				Action = MessageBoxAction.Ok,
				Icon = MessageBoxIcon.Warning,
				Message = TextKey.MessageBox_ServerReconfigurationWarning,
				Title = TextKey.MessageBox_ServerReconfigurationWarningTitle
			};
			logger.Warn("Server reconfiguration requested but is not allowed.");

			ActionRequired?.Invoke(args);
		}
	}
}
