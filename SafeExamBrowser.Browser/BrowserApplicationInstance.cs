/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationInstance : IApplicationInstance
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public IWindow Window { get; private set; }

		public BrowserApplicationInstance(string name)
		{
			Id = Guid.NewGuid();
			Name = name;
		}

		public void RegisterWindow(IWindow window)
		{
			Window = window;
		}
	}
}
