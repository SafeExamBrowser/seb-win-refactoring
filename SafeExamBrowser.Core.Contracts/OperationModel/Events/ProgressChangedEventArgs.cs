/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.OperationModel.Events
{
	/// <summary>
	/// The event arguments used for <see cref="IOperationSequence.ProgressChanged"/>.
	/// </summary>
	public class ProgressChangedEventArgs
	{
		/// <summary>
		/// Specifies the current progress value, if set.
		/// </summary>
		public int? CurrentValue { get; set; }

		/// <summary>
		/// Indicates that the progress value is indeterminate, if set.
		/// </summary>
		public bool? IsIndeterminate { get; set; }

		/// <summary>
		/// Sets the maximum progress value, if set.
		/// </summary>
		public int? MaxValue { get; set; }

		/// <summary>
		/// Indicates that the current progress value has increased by 1, if set.
		/// </summary>
		public bool? Progress { get; set; }

		/// <summary>
		/// Indicates that the current progress value has decreased by 1, if set.
		/// </summary>
		public bool? Regress { get; set; }
	}
}
