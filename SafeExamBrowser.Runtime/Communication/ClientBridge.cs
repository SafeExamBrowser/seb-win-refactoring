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
using System.Threading;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Communication
{
	/// <summary>
	/// The client bridge simplifies the communication resp. user interaction resolution with the client application component.
	/// </summary>
	internal class ClientBridge
	{
		private readonly IRuntimeHost runtimeHost;
		private readonly RuntimeContext runtimeContext;

		internal ClientBridge(IRuntimeHost runtimeHost, RuntimeContext runtimeContext)
		{
			this.runtimeHost = runtimeHost;
			this.runtimeContext = runtimeContext;
		}

		internal bool IsRequired()
		{
			var session = runtimeContext.Current;
			var isStartup = session == default;
			var isRunningOnDefaultDesktop = session != default && session.Settings.Security.KioskMode != KioskMode.CreateNewDesktop;

			return !isStartup && !isRunningOnDefaultDesktop;
		}

		internal MessageBoxResult ShowMessageBox(string message, string title, MessageBoxAction action, MessageBoxIcon icon)
		{
			var requestId = Guid.NewGuid();
			var result = MessageBoxResult.None;
			var response = default(MessageBoxReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<MessageBoxReplyEventArgs>((args) =>
			{
				if (args.RequestId == requestId)
				{
					response = args;
					responseEvent.Set();
				}
			});

			runtimeHost.MessageBoxReplyReceived += responseEventHandler;

			var communication = runtimeContext.ClientProxy.ShowMessage(message, title, (int) action, (int) icon, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				result = (MessageBoxResult) response.Result;
			}

			runtimeHost.MessageBoxReplyReceived -= responseEventHandler;

			return result;
		}

		internal bool TryAskForExamSelection(IEnumerable<Exam> exams, out Exam exam)
		{
			var success = false;
			var requestId = Guid.NewGuid();
			var response = default(ExamSelectionReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<ExamSelectionReplyEventArgs>((a) =>
			{
				if (a.RequestId == requestId)
				{
					response = a;
					responseEvent.Set();
				}
			});

			exam = default;
			runtimeHost.ExamSelectionReceived += responseEventHandler;

			var availableExams = exams.Select(e => (e.Id, e.LmsName, e.Name, e.Url));
			var communication = runtimeContext.ClientProxy.RequestExamSelection(availableExams, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				exam = exams.First(e => e.Id == response.SelectedExamId);
				success = response.Success;
			}

			runtimeHost.ExamSelectionReceived -= responseEventHandler;

			return success;
		}

		internal void TryAskForServerFailureAction(string message, bool showFallback, out bool abort, out bool fallback, out bool retry)
		{
			var requestId = Guid.NewGuid();
			var response = default(ServerFailureActionReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<ServerFailureActionReplyEventArgs>((a) =>
			{
				if (a.RequestId == requestId)
				{
					response = a;
					responseEvent.Set();
				}
			});

			runtimeHost.ServerFailureActionReceived += responseEventHandler;

			var communication = runtimeContext.ClientProxy.RequestServerFailureAction(message, showFallback, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				abort = response.Abort;
				fallback = response.Fallback;
				retry = response.Retry;
			}
			else
			{
				abort = true;
				fallback = false;
				retry = false;
			}

			runtimeHost.ServerFailureActionReceived -= responseEventHandler;
		}

		internal bool TryGetPassword(PasswordRequestPurpose purpose, out string password)
		{
			var success = false;
			var requestId = Guid.NewGuid();
			var response = default(PasswordReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<PasswordReplyEventArgs>((a) =>
			{
				if (a.RequestId == requestId)
				{
					response = a;
					responseEvent.Set();
				}
			});

			password = default;
			runtimeHost.PasswordReceived += responseEventHandler;

			var communication = runtimeContext.ClientProxy.RequestPassword(purpose, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				password = response.Password;
				success = response.Success;
			}

			runtimeHost.PasswordReceived -= responseEventHandler;

			return success;
		}
	}
}
