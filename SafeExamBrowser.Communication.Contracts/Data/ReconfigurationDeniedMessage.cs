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
	/// This message is transmitted from the runtime to the client in order to inform the latter that a reconfiguration request was denied.
	/// </summary>
	[Serializable]
	public class ReconfigurationDeniedMessage : Message
	{
		/// <summary>
		/// The full path to the configuration file for which a reconfiguration was denied.
		/// </summary>
		public string FilePath { get; private set; }

		public ReconfigurationDeniedMessage(string filePath)
		{
			FilePath = filePath;
		}
	}
}
