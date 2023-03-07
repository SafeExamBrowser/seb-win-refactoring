/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts
{
	/// <summary>
	/// Defines all possible interlocutors for inter-process communication within the application.
	/// </summary>
	[Serializable]
	public enum Interlocutor
	{
		/// <summary>
		/// The interlocutor is not an application component.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The client application component.
		/// </summary>
		Client,

		/// <summary>
		/// The runtime application component.
		/// </summary>
		Runtime,

		/// <summary>
		/// The service application component.
		/// </summary>
		Service
	}
}
