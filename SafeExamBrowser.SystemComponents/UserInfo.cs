/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.SystemComponents
{
	public class UserInfo : IUserInfo
	{
		private const string SID_REGEX_PATTERN = @"S-\d(-\d+)+";

		private ILogger logger;

		public UserInfo(ILogger logger)
		{
			this.logger = logger;
		}

		public string GetUserName()
		{
			return Environment.UserName;
		}

		public string GetUserSid()
		{
			return WindowsIdentity.GetCurrent().User.Value;
		}

		public bool TryGetSidForUser(string userName, out string sid)
		{
			var strategies = new Func<string, string>[] { NtAccount, Wmi };
			var success = false;

			sid = default(string);

			foreach (var strategy in strategies)
			{
				try
				{
					sid = strategy.Invoke(userName);

					if (IsValid(sid))
					{
						logger.Info($"Found SID '{sid}' via '{strategy.Method.Name}' for user name '{userName}'!");
						success = true;

						break;
					}

					logger.Warn($"Retrieved invalid SID '{sid}' via '{strategy.Method.Name}' for user name '{userName}'!");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to get SID via '{strategy.Method.Name}' for user name '{userName}'!", e);
				}
			}

			if (!success)
			{
				logger.Error($"All attempts to retrieve SID for user name '{userName}' failed!");
			}

			return success;
		}

		private string NtAccount(string userName)
		{
			var account = new NTAccount(userName);

			if (account.IsValidTargetType(typeof(SecurityIdentifier)))
			{
				return account.Translate(typeof(SecurityIdentifier)).Value;
			}

			return null;
		}

		private string Wmi(string userName)
		{
			var process = new Process();

			process.StartInfo.Arguments = $"/c \"wmic useraccount where name='{userName}' get sid\"";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			process.Start();
			process.WaitForExit(5000);

			var output = process.StandardOutput.ReadToEnd();
			var match = Regex.Match(output, SID_REGEX_PATTERN);

			return match.Success ? match.Value : null;
		}

		private bool IsValid(string sid)
		{
			return !String.IsNullOrWhiteSpace(sid) && Regex.IsMatch(sid, $"^{SID_REGEX_PATTERN}$");
		}
	}
}
