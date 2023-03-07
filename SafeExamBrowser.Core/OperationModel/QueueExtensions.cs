/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Core.OperationModel
{
	internal static class QueueExtensions
	{
		internal static void ForEach<T>(this Queue<T> queue, Action<T> action)
		{
			foreach (var element in queue)
			{
				action(element);
			}
		}
	}
}
