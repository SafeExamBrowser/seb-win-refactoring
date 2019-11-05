/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Defines an identifier which uniquely identifies an instance in the context of an application.
	/// </summary>
	public abstract class InstanceIdentifier
	{
		/// <summary>
		/// Determines whether two identifiers are equal (i.e. whether they identify the same application instance).
		/// </summary>
		public static bool operator ==(InstanceIdentifier a, InstanceIdentifier b) => Equals(a, b);

		/// <summary>
		/// Determines whether two identifiers are different (i.e. whether they identify different application instances).
		/// </summary>
		public static bool operator !=(InstanceIdentifier a, InstanceIdentifier b) => !Equals(a, b);

		/// <summary>
		/// Indicates whether the given object is an identifier for the same application instance.
		/// </summary>
		public abstract override bool Equals(object other);

		/// <summary>
		/// Returns a hash code for the identifier.
		/// </summary>
		public abstract override int GetHashCode();

		/// <summary>
		/// Returns a human-readable string representation of the identifier.
		/// </summary>
		public abstract override string ToString();
	}
}
