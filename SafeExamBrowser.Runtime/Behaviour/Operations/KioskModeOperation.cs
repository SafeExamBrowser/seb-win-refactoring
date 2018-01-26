/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class KioskModeOperation : IOperation
	{
		public bool AbortStartup { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public void Perform()
		{
			// TODO
		}

		public void Revert()
		{
			// TODO
		}
	}
}
