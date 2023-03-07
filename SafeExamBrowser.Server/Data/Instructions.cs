/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Data
{
	internal sealed class Instructions
	{
		internal const string LOCK_SCREEN = "SEB_FORCE_LOCK_SCREEN";
		internal const string NOTIFICATION_CONFIRM = "NOTIFICATION_CONFIRM";
		internal const string PROCTORING = "SEB_PROCTORING";
		internal const string PROCTORING_RECONFIGURATION = "SEB_RECONFIGURE_SETTINGS";
		internal const string QUIT = "SEB_QUIT";
	}
}
