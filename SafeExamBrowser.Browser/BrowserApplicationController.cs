/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationController : IApplicationController
	{
		private IApplicationButton button;

		public void RegisterApplicationButton(IApplicationButton button)
		{
			this.button = button;
			this.button.OnClick += ButtonClick;
		}

		private void ButtonClick(Guid? instanceId = null)
		{
			button.RegisterInstance(new BrowserApplicationInstance("A new instance. Yaji..."));
		}
	}
}
