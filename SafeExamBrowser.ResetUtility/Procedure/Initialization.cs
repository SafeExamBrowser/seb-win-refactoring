/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Security.Principal;
using System.Threading;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Initialization : ProcedureStep
	{
		private static readonly Mutex mutex = new Mutex(true, AppConfig.SERVICE_MUTEX_NAME);

		public Initialization(ProcedureContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			InitializeConsole();

			if (IsSingleInstance() && HasAdminPrivileges() && SebNotRunning())
			{
				return ProcedureStepResult.Continue;
			}
			else
			{
				return ProcedureStepResult.Terminate;
			}
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
		}

		private bool IsSingleInstance()
		{
			var isSingle = mutex.WaitOne(TimeSpan.Zero, true);

			if (isSingle)
			{
				Logger.Info("There is currently no other instance running.");
			}
			else
			{
				Logger.Error("There is currently another instance running! Terminating...");
				ShowError("You can only run one instance of the Reset Utility at a time! Press any key to exit...");
			}

			return isSingle;
		}

		private bool HasAdminPrivileges()
		{
			var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

			if (isAdmin)
			{
				Logger.Info($"User '{identity.Name}' is running the application with administrator privileges.");
			}
			else
			{
				Logger.Error($"User '{identity.Name}' is running the application without administrator privileges! Terminating...");
				ShowError("This application must be run with administrator privileges! Press any key to exit...");
			}

			return isAdmin;
		}

		private bool SebNotRunning()
		{
			var isRunning = Mutex.TryOpenExisting(AppConfig.RUNTIME_MUTEX_NAME, out _);

			if (isRunning)
			{
				Logger.Error("SEB is currently running! Terminating...");
				ShowError("This application must not be run while SEB is running! Press any key to exit...");
			}
			else
			{
				Logger.Info("SEB is currently not running.");
			}

			return !isRunning;
		}
	}
}
