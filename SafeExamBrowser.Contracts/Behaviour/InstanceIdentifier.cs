/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Behaviour
{
	/// <summary>
	/// Defines an identifier which uniquely identifies an <see cref="IApplicationInstance"/> in the context of a (third-party) application.
	/// </summary>
	public abstract class InstanceIdentifier
	{
		/// <summary>
		/// Determines whether two identifiers are equal (i.e. whether they identify the same <see cref="IApplicationInstance"/>).
		/// </summary>
		public static bool operator ==(InstanceIdentifier a, InstanceIdentifier b) => Equals(a, b);

		/// <summary>
		/// Determines whether two identifiers are different (i.e. whether they identify different <see cref="IApplicationInstance"/>s).
		/// </summary>
		public static bool operator !=(InstanceIdentifier a, InstanceIdentifier b) => !Equals(a, b);

		/// <summary>
		/// Indicates whether the given object is an <see cref="InstanceIdentifier"/> for the same <see cref="IApplicationInstance"/>.
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
