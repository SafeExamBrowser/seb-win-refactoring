/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Windows.Threading;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;
using SafeExamBrowser.SystemComponents.PreExamCheck;
using SafeExamBrowser.UserInterface.Desktop.Windows;

namespace SafeExamBrowser.Client.Operations
{
	internal class PreExamCheckOperation : ClientOperation
	{
		private readonly ILogger logger;
		private readonly ISystemInfo systemInfo;
		private readonly IPreExamCheckService checkService;

		public override event StatusChangedEventHandler StatusChanged;

		public PreExamCheckOperation(ClientContext context, ILogger logger, ISystemInfo systemInfo, IPreExamCheckService checkService = null) : base(context)
		{
			this.logger = logger;
			this.systemInfo = systemInfo;
			this.checkService = checkService ?? new PreExamCheckService(systemInfo);
		}

		public override OperationResult Perform()
		{
			logger.Info("Executing Pre-Exam System Check...");

			var report = checkService.RunAllChecks();
			logger.Info($"Pre-Exam System Check completed. (Passed Critical: {report.PassedAllCritical}, Has Warnings: {report.HasWarnings})");

			bool userProceeded = false;

			// Show PreExamCheckDialog on UI thread
			var thread = new Thread(() =>
			{
				var dialog = new PreExamCheckDialog(checkService, new SafeExamBrowser.SystemComponents.PreExamCheck.Reporting.ReportGenerator());
				dialog.ShowDialog();
				userProceeded = dialog.UserProceeded;
				Dispatcher.CurrentDispatcher.InvokeShutdown();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();

			if (userProceeded)
			{
				logger.Info("User confirmed and proceeded after Pre-Exam System Check.");
				return OperationResult.Success;
			}

			logger.Error("Pre-Exam System Check failed or candidate cancelled exam start.");
			return OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
