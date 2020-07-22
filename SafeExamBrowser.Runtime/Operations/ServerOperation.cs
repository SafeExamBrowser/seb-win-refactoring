/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServerOperation : ConfigurationBaseOperation
	{
		private readonly ILogger logger;
		private readonly IServerProxy server;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			ILogger logger,
			SessionContext context,
			IServerProxy server) : base(commandLineArgs, configuration, context)
		{
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

				var (abort, fallback, success) = TryPerformWithFallback(() => server.Connect(), out var token);

				if (success)
				{
					(abort, fallback, success) = TryPerformWithFallback(() => server.GetAvailableExams(), out var exams);

					if (success)
					{
						success = TrySelectExam(exams, out var exam);

						if (success)
						{
							(abort, fallback, success) = TryPerformWithFallback(() => server.GetConfigurationFor(exam), out var uri);

							if (success)
							{
								var status = TryLoadSettings(uri, UriSource.Server, out _, out var settings);

								if (status == LoadStatus.Success)
								{
									Context.Next.Settings = settings;
									result = OperationResult.Success;
								}
								else
								{
									result = OperationResult.Failed;
								}
							}
						}
						else
						{
							logger.Info("The user aborted the exam selection.");
							result = OperationResult.Aborted;
						}
					}
				}

				if (abort)
				{
					result = OperationResult.Aborted;
				}

				if (fallback)
				{
					result = OperationResult.Success;
				}
			}

			return result;
		}

		public override OperationResult Repeat()
		{
			var result = Revert();

			if (result == OperationResult.Success)
			{
				result = Perform();
			}

			return result;
		}

		public override OperationResult Revert()
		{
			var result = OperationResult.Failed;

			if (Context.Current.Settings.SessionMode == SessionMode.Server)
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
			else
			{
				result = OperationResult.Success;
			}

			return result;
		}

		protected override void InvokeActionRequired(ActionRequiredEventArgs args)
		{
			ActionRequired?.Invoke(args);
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

			value = default(T);

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
			var args = new ServerFailureEventArgs(message);

			ActionRequired?.Invoke(args);

			abort = args.Abort;
			fallback = args.Fallback;

			return args.Retry;
		}

		private bool TrySelectExam(IEnumerable<Exam> exams, out Exam exam)
		{
			var args = new ExamSelectionEventArgs(exams);

			ActionRequired?.Invoke(args);
			exam = args.SelectedExam;

			return args.Success;
		}
	}
}
