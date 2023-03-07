/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the server exam selection request event fired by the <see cref="Hosts.IClientHost"/>.
	/// </summary>
	public class ExamSelectionRequestEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The exams from which the user needs to make a selection.
		/// </summary>
		public IEnumerable<(string id, string lms, string name, string url)> Exams { get; set; }

		/// <summary>
		/// Identifies the server exam selection request.
		/// </summary>
		public Guid RequestId { get; set; }
	}
}
