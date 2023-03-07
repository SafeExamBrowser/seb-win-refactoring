/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.UserInterface.Contracts.Browser.Data
{
	/// <summary>
	/// Defines the state of a download item.
	/// </summary>
	public class DownloadItemState
	{
		/// <summary>
		/// The current completion of the item, as percentage value from <c>0.0</c> to <c>1.0</c>.
		/// </summary>
		public double Completion { get; set; }

		/// <summary>
		/// The full path of the download location for the item.
		/// </summary>
		public string FullPath { get; set; }

		/// <summary>
		/// The unique identifier of the item.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Indicates that the download was cancelled.
		/// </summary>
		public bool IsCancelled { get; set; }

		/// <summary>
		/// Indicates that the download was completed.
		/// </summary>
		public bool IsComplete { get; set; }

		/// <summary>
		/// The download URL of the item.
		/// </summary>
		public string Url { get; set; }

		public DownloadItemState(Guid id)
		{
			Id = id;
		}
	}
}
