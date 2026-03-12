/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.ResponsibilityModel
{
	/// <summary>
	/// An unordered collection of <see cref="IResponsibility{T}"/> which can be used to separate concerns, e.g. functionalities of an application
	/// component. Each task delegation will be executed failsafe, i.e. the delegation will continue even if a particular responsibility fails while
	/// assuming a task.
	/// </summary>
	public interface IResponsibilityCollection<T>
	{
		/// <summary>
		/// Delegates the given task to all responsibilities of the collection.
		/// </summary>
		void Delegate(T task);

		/// <summary>
		/// Delegates the given task with the requested return value type to all functional responsibilities of the collection.
		/// Returns <c>default(TResult)</c> in case there is no responsibility which can assume the given task.
		/// </summary>
		TResult Delegate<TResult>(T task) where TResult : class;
	}
}
