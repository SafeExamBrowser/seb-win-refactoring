/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using SafeExamBrowser.Client.Contracts;

namespace SafeExamBrowser.Client
{
	internal class Coordinator : ICoordinator
	{
		private readonly ConcurrentBag<Guid> reconfiguration;
		private readonly ConcurrentBag<Guid> session;

		internal Coordinator()
		{
			reconfiguration = new ConcurrentBag<Guid>();
			session = new ConcurrentBag<Guid>();
		}

		public bool IsReconfigurationLocked()
		{
			return !reconfiguration.IsEmpty;
		}

		public bool IsSessionLocked()
		{
			return !session.IsEmpty;
		}

		public void ReleaseReconfigurationLock()
		{
			reconfiguration.TryTake(out _);
		}

		public void ReleaseSessionLock()
		{
			session.TryTake(out _);
		}

		public bool RequestReconfigurationLock()
		{
			var acquired = false;

			lock (reconfiguration)
			{
				if (reconfiguration.IsEmpty)
				{
					reconfiguration.Add(Guid.NewGuid());
					acquired = true;
				}
			}

			return acquired;
		}

		public bool RequestSessionLock()
		{
			var acquired = false;

			lock (session)
			{
				if (session.IsEmpty)
				{
					session.Add(Guid.NewGuid());
					acquired = true;
				}
			}

			return acquired;
		}
	}
}
