/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Server.Contracts.Data;

namespace SafeExamBrowser.Runtime.Operations.Events
{
	internal class ExamSelectionEventArgs : ActionRequiredEventArgs
	{
		internal IEnumerable<Exam> Exams { get; set; }
		internal Exam SelectedExam { get; set; }
		internal bool Success { get; set; }

		internal ExamSelectionEventArgs(IEnumerable<Exam> exams)
		{
			Exams = exams;
		}
	}
}
