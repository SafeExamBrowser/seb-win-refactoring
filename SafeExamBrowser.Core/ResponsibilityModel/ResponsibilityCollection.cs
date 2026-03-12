/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.ResponsibilityModel
{
	/// <summary>
	/// Default implementation of the <see cref="IResponsibilityCollection{T}"/>.
	/// </summary>
	public class ResponsibilityCollection<T> : IResponsibilityCollection<T>
	{
		protected ILogger logger;
		protected Queue<IResponsibility<T>> responsibilities;

		public ResponsibilityCollection(ILogger logger, IEnumerable<IResponsibility<T>> responsibilities)
		{
			this.logger = logger;
			this.responsibilities = new Queue<IResponsibility<T>>(responsibilities);
		}

		public void Delegate(T task)
		{
			foreach (var responsibility in responsibilities)
			{
				try
				{
					responsibility.Assume(task);
				}
				catch (Exception e)
				{
					logger.Error($"Caught unexpected exception while '{responsibility.GetType().Name}' was assuming task '{task}'!", e);
				}
			}
		}

		public TResult Delegate<TResult>(T task) where TResult : class
		{
			var result = default(TResult);

			foreach (var responsibility in responsibilities.OfType<IFunctionalResponsibility<T>>())
			{
				try
				{
					if (responsibility.TryAssume(task, out result))
					{
						break;
					}
				}
				catch (Exception e)
				{
					logger.Error($"Caught unexpected exception while '{responsibility.GetType().Name}' was assuming task '{task}' with return value type '{typeof(TResult).Name}'!", e);
				}
			}

			return result;
		}
	}
}
