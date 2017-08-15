/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.SystemComponents
{
	public class PowerSupply : ISystemComponent<ISystemPowerSupplyControl>
	{
		private ILogger logger;
		private ISystemPowerSupplyControl control;

		public PowerSupply(ILogger logger)
		{
			this.logger = logger;
		}

		public void Initialize()
		{
			
		}

		public void RegisterControl(ISystemPowerSupplyControl control)
		{
			this.control = control;
		}

		public void Terminate()
		{
			
		}
	}
}
