/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	class CompositionRoot
	{
		public Window Taskbar { get; private set; }

		public void InitializeGlobalModules()
		{
			Text.Instance = new Text(new XmlTextResource());
		}

		public void BuildObjectGraph()
		{
			Taskbar = new Taskbar();
		}
	}
}
