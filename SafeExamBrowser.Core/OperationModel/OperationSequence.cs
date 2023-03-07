/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.OperationModel
{
	/// <summary>
	/// Default implementation of the <see cref="IOperationSequence"/>.
	/// </summary>
	public class OperationSequence : IOperationSequence
	{
		protected ILogger logger;
		protected Queue<IOperation> operations = new Queue<IOperation>();
		protected Stack<IOperation> stack = new Stack<IOperation>();

		public event ActionRequiredEventHandler ActionRequired
		{
			add { operations.ForEach(o => o.ActionRequired += value); }
			remove { operations.ForEach(o => o.ActionRequired -= value); }
		}

		public event ProgressChangedEventHandler ProgressChanged;

		public event StatusChangedEventHandler StatusChanged
		{
			add { operations.ForEach(o => o.StatusChanged += value); }
			remove { operations.ForEach(o => o.StatusChanged -= value); }
		}

		public OperationSequence(ILogger logger, Queue<IOperation> operations)
		{
			this.logger = logger;
			this.operations = new Queue<IOperation>(operations);
		}

		public virtual OperationResult TryPerform()
		{
			var result = OperationResult.Failed;

			try
			{
				Initialize();
				result = Perform();

				if (result != OperationResult.Success)
				{
					Revert(true);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to perform operations!", e);
			}

			return result;
		}

		public virtual OperationResult TryRevert()
		{
			var result = OperationResult.Failed;

			try
			{
				Initialize(true);
				result = Revert();
			}
			catch (Exception e)
			{
				logger.Error("Failed to revert operations!", e);
			}

			return result;
		}

		protected virtual void Initialize(bool indeterminate = false)
		{
			if (indeterminate)
			{
				UpdateProgress(new ProgressChangedEventArgs { IsIndeterminate = true });
			}
			else
			{
				UpdateProgress(new ProgressChangedEventArgs { CurrentValue = 0, MaxValue = operations.Count });
			}
		}

		protected virtual OperationResult Perform()
		{
			foreach (var operation in operations)
			{
				var result = OperationResult.Failed;

				stack.Push(operation);

				try
				{
					result = operation.Perform();
				}
				catch (Exception e)
				{
					logger.Error($"Caught unexpected exception while performing operation '{operation.GetType().Name}'!", e);
				}

				if (result != OperationResult.Success)
				{
					return result;
				}

				UpdateProgress(new ProgressChangedEventArgs { Progress = true });
			}

			return OperationResult.Success;
		}

		protected virtual OperationResult Revert(bool regress = false)
		{
			var success = true;

			while (stack.Any())
			{
				var operation = stack.Pop();

				try
				{
					var result = operation.Revert();

					if (result != OperationResult.Success)
					{
						success = false;
					}
				}
				catch (Exception e)
				{
					logger.Error($"Caught unexpected exception while reverting operation '{operation.GetType().Name}'!", e);
					success = false;
				}

				if (regress)
				{
					UpdateProgress(new ProgressChangedEventArgs { Regress = true });
				}
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		protected void UpdateProgress(ProgressChangedEventArgs args)
		{
			ProgressChanged?.Invoke(args);
		}
	}
}
