/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.NetworkInformation;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class InternetChecker : IChecker
	{
		public string Name => "Connect to Internet";

		public CheckResult Check()
		{
			bool isConnected = NetworkInterface.GetIsNetworkAvailable();

			return new CheckResult
			{
				Category = "Network",
				Title = Name,
				Status = isConnected ? CheckStatus.Passed : CheckStatus.Failed,
				ActualValue = isConnected ? "Connected" : "Not connected",
				RequiredValue = "Connected to network",
				Message = isConnected ? "Network connection is working properly." : "No valid Internet connection found.",
				IsCritical = true
			};
		}
	}
}
