/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ApplicationInstanceIdentifier : InstanceIdentifier
	{
		internal int ProcessId { get; private set; }

		public ApplicationInstanceIdentifier(int processId)
		{
			ProcessId = processId;
		}

		public override bool Equals(object other)
		{
			if (other is ApplicationInstanceIdentifier id)
			{
				return ProcessId == id.ProcessId;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ProcessId.GetHashCode();
		}

		public override string ToString()
		{
			return $"({ProcessId})";
		}
	}
}
