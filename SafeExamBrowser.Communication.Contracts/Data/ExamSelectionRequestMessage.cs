/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// This message is transmitted to the client to request a server exam selection by the user.
	/// </summary>
	[Serializable]
	public class ExamSelectionRequestMessage : Message
	{
		/// <summary>
		/// The exams from which the user needs to make a selection.
		/// </summary>
		public IEnumerable<(string id, string lms, string name, string url)> Exams { get; }

		/// <summary>
		/// The unique identifier for the server exam selection request.
		/// </summary>
		public Guid RequestId { get; }

		public ExamSelectionRequestMessage(IEnumerable<(string id, string lms, string name, string url)> exams, Guid requestId)
		{
			Exams = exams;
			RequestId = requestId;
		}
	}
}
