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
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplicationWindow : IApplicationWindow
	{
		private INativeMethods nativeMethods;

		internal IntPtr Handle { get; }
		public IconResource Icon { get; }
		public string Title { get; private set; }

		public event IconChangedEventHandler IconChanged { add { } remove { } }
		public event TitleChangedEventHandler TitleChanged;

		internal ExternalApplicationWindow(IconResource icon, INativeMethods nativeMethods, IntPtr handle)
		{
			this.Handle = handle;
			this.Icon = icon;
			this.nativeMethods = nativeMethods;
		}

		public void Activate()
		{
			nativeMethods.ActivateWindow(Handle);
		}

		internal void Update()
		{
			var title = nativeMethods.GetWindowTitle(Handle);
			var hasChanged = Title?.Equals(title, StringComparison.Ordinal) != true;

			if (hasChanged)
			{
				Title = title;
				TitleChanged?.Invoke(title);
			}
		}
	}
}
