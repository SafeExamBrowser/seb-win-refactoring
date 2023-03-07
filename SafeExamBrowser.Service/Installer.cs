/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.ServiceProcess;

namespace SafeExamBrowser.Service
{
	[RunInstaller(true)]
	public class Installer : System.Configuration.Install.Installer
	{
		private ServiceProcessInstaller process;
		private ServiceInstaller service;

		public Installer()
		{
			process = new ServiceProcessInstaller();
			process.Account = ServiceAccount.LocalSystem;

			service = new ServiceInstaller();
			service.Description = "Performs operations which require elevated privileges.";
			service.DisplayName = "Safe Exam Browser Service";
			service.ServiceName = nameof(SafeExamBrowser);
			service.StartType = ServiceStartMode.Automatic;

			Installers.Add(process);
			Installers.Add(service);
		}
	}
}
