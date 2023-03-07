/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	internal class EventStub : EventWaitHandle
	{
		public bool IsClosed { get; set; }

		public EventStub() : base(false, EventResetMode.AutoReset)
		{
		}

		public override void Close()
		{
			IsClosed = true;
		}
	}
}
