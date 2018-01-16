/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Logging
{
	public class LogText : ILogText
	{
		public string Text { get; private set; }

		public LogText(string text)
		{
			Text = text;
		}

		public object Clone()
		{
			return new LogText(Text);
		}
	}
}
