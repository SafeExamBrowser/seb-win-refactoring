/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplication : IApplication
	{
		public IApplicationInfo Info => throw new NotImplementedException();

		public event InstanceStartedEventHandler InstanceStarted;

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
