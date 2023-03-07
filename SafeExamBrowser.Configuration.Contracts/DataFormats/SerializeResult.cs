/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace SafeExamBrowser.Configuration.Contracts.DataFormats
{
	/// <summary>
	/// Defines the result of a data serialization operation by an <see cref="IDataSerializer"/>.
	/// </summary>
	public class SerializeResult
	{
		/// <summary>
		/// The serialized data. Might be <c>null</c> or in an undefinable state, depending on <see cref="Status"/>.
		/// </summary>
		public Stream Data { get; set; }

		/// <summary>
		/// The status result of the serialization operation.
		/// </summary>
		public SaveStatus Status { get; set; }
	}
}
