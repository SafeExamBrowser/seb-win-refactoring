/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal abstract class ClientResponsibility : IResponsibility<ClientTask>
	{
		protected ClientContext Context { get; private set; }
		protected ILogger Logger { get; private set; }

		protected AppSettings Settings => Context.Settings;

		internal ClientResponsibility(ClientContext context, ILogger logger)
		{
			Context = context;
			Logger = logger;
		}

		public abstract void Assume(ClientTask task);

		protected void PauseActivators()
		{
			foreach (var activator in Context.Activators)
			{
				activator.Pause();
			}
		}

		protected void PrepareShutdown()
		{
			Context.Responsibilities.Delegate(ClientTask.PrepareShutdown_Wave1);
			Context.Responsibilities.Delegate(ClientTask.PrepareShutdown_Wave2);
		}

		protected void ResumeActivators()
		{
			foreach (var activator in Context.Activators)
			{
				activator.Resume();
			}
		}

		protected LockScreenResult ShowLockScreen(string message, string title, IEnumerable<LockScreenOption> options)
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings.Security.QuitPasswordHash);
			var result = default(LockScreenResult);

			Context.LockScreen = Context.UserInterfaceFactory.CreateLockScreen(message, title, options, Settings.UserInterface.LockScreen);
			Logger.Info("Showing lock screen...");

			PauseActivators();
			Context.LockScreen.Show();

			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Context.Server.LockScreen(message);

				if (!response.Success)
				{
					Logger.Error($"Failed to send lock screen notification to server! Message: {response.Message}.");
				}
			}

			for (var unlocked = false; !unlocked;)
			{
				result = Context.LockScreen.WaitForResult();

				if (result.Canceled)
				{
					Logger.Info("The lock screen has been automaticaly canceled.");
					unlocked = true;
				}
				else if (hasQuitPassword)
				{
					var passwordHash = Context.HashAlgorithm.GenerateHashFor(result.Password);
					var isCorrect = Settings.Security.QuitPasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);

					if (isCorrect)
					{
						Logger.Info("The user entered the correct unlock password.");
						unlocked = true;
					}
					else
					{
						Logger.Info("The user entered the wrong unlock password.");
						Context.MessageBox.Show(TextKey.MessageBox_InvalidUnlockPassword, TextKey.MessageBox_InvalidUnlockPasswordTitle, icon: MessageBoxIcon.Warning, parent: Context.LockScreen);
					}
				}
				else
				{
					Logger.Warn($"No unlock password is defined, allowing user to resume session!");
					unlocked = true;
				}
			}

			Context.LockScreen.Close();
			ResumeActivators();

			Logger.Info("Closed lock screen.");

			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Context.Server.ConfirmLockScreen();

				if (!response.Success)
				{
					Logger.Error($"Failed to send lock screen confirm notification to server! Message: {response.Message}.");
				}
			}

			return result;
		}

		protected bool TryRequestShutdown()
		{
			PrepareShutdown();

			var communication = Context.Runtime.RequestShutdown();

			if (!communication.Success)
			{
				Logger.Error("Failed to communicate shutdown request to the runtime!");
				Context.MessageBox.Show(TextKey.MessageBox_QuitError, TextKey.MessageBox_QuitErrorTitle, icon: MessageBoxIcon.Error);
			}

			return communication.Success;
		}
	}
}
