/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Suspension
{
	internal class MonotonicTime
	{
		private readonly INativeMethods nativeMethods;

		internal TimeSpan Now => TimeSpan.FromTicks(nativeMethods.QueryUnbiasedInterruptTime());

		internal MonotonicTime(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
		}
	}
}
