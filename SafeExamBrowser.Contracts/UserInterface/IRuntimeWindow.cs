/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Contracts.UserInterface
{
	public interface IRuntimeWindow : ILogObserver, IWindow
	{
		/// <summary>
		/// Updates the status text of the runtime window. If the busy flag is set,
		/// the window will show an animation to indicate a long-running operation.
		/// </summary>
		void UpdateStatus(TextKey key, bool showBusyIndication = false);
	}
}
