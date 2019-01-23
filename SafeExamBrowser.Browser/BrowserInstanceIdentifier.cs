/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Applications;

namespace SafeExamBrowser.Browser
{
	internal class BrowserInstanceIdentifier : InstanceIdentifier
	{
		public int Value { get; private set; }

		public BrowserInstanceIdentifier(int id)
		{
			Value = id;
		}

		public override bool Equals(object other)
		{
			if (other is BrowserInstanceIdentifier id)
			{
				return Value == id.Value;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return $"#{Value}";
		}
	}
}
