/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Logging
{
	/// <summary>
	/// Default implementation of <see cref="IThreadInfo"/>.
	/// </summary>
	public class ThreadInfo : IThreadInfo
	{
		public int Id { get; private set; }
		public string Name { get; private set; }

		public bool HasName
		{
			get { return !String.IsNullOrWhiteSpace(Name); }
		}

		public ThreadInfo(int id, string name = null)
		{
			Id = id;
			Name = name;
		}

		public object Clone()
		{
			return new ThreadInfo(Id, Name);
		}
	}
}
