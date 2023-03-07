/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceProcess;

namespace SafeExamBrowser.Service
{
	public class Service : ServiceBase
	{
		private CompositionRoot instances;

		public Service()
		{
			CanPauseAndContinue = false;
			ServiceName = nameof(SafeExamBrowser);
		}

		public static void Main()
		{
			Run(new Service());
		}

		protected override void OnStart(string[] args)
		{
			instances = new CompositionRoot();
			instances.BuildObjectGraph();
			instances.LogStartupInformation();

			var success = instances.ServiceController.TryStart();

			if (!success)
			{
				Environment.Exit(-1);
			}
		}

		protected override void OnStop()
		{
			instances?.ServiceController?.Terminate();
			instances?.LogShutdownInformation();
		}
	}
}
