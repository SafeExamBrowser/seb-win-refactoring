/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the exam selection event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class ExamSelectionReplyEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// Identifies the exam selection request.
		/// </summary>
		public Guid RequestId { get; set; }

		/// <summary>
		/// The identifier of the exam selected by the user.
		/// </summary>
		public string SelectedExamId { get; set; }

		/// <summary>
		/// Indicates whether an exam has been successfully selected by the user.
		/// </summary>
		public bool Success { get; set; }
	}
}
