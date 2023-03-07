/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// The reply to a <see cref="ExamSelectionRequestMessage"/>.
	/// </summary>
	[Serializable]
	public class ExamSelectionReplyMessage : Message
	{
		/// <summary>
		/// The unique identifier for the exam selection request.
		/// </summary>
		public Guid RequestId { get; }

		/// <summary>
		/// The identifier of the exam selected by the user.
		/// </summary>
		public string SelectedExamId { get; }

		/// <summary>
		/// Determines whether the user interaction was successful or not.
		/// </summary>
		public bool Success { get; }

		public ExamSelectionReplyMessage(Guid requestId, bool success, string selectedExamId)
		{
			RequestId = requestId;
			Success = success;
			SelectedExamId = selectedExamId;
		}
	}
}
