/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.ResponsibilityModel
{
	/// <summary>
	/// Defines a responsibility which will be executed as part of an <see cref="IResponsibilityCollection{T}"/>.
	/// </summary>
	public interface IResponsibility<T>
	{
		/// <summary>
		/// Assumes the given task.
		/// </summary>
		void Assume(T task);
	}
}
