/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplication : IApplication
	{
		private string executablePath;

		public ApplicationInfo Info { get; }

		public event InstanceStartedEventHandler InstanceStarted;

		internal ExternalApplication(string executablePath, ApplicationInfo info)
		{
			this.executablePath = executablePath;
			this.Info = info;
		}

		public void Initialize()
		{
			
		}

		public void Start()
		{
			
		}

		public void Terminate()
		{
			
		}
	}
}
