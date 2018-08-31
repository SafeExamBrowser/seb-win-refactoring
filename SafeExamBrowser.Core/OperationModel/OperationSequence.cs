/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.OperationModel
{
	/// <summary>
	/// Default implementation of the <see cref="IOperationSequence"/>.
	/// </summary>
	public class OperationSequence : IOperationSequence
	{
		private ILogger logger;
		private Queue<IOperation> operations = new Queue<IOperation>();
		private Stack<IOperation> stack = new Stack<IOperation>();

		public IProgressIndicator ProgressIndicator { private get; set; }

		public OperationSequence(ILogger logger, Queue<IOperation> operations)
		{
			this.logger = logger;
			this.operations = new Queue<IOperation>(operations);
		}

		public OperationResult TryPerform()
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

		public OperationResult TryRepeat()
		{
			var result = OperationResult.Failed;

			try
			{
				Initialize();
				result = Repeat();
			}
			catch (Exception e)
			{
				logger.Error("Failed to repeat operations!", e);
			}

			return result;
		}

		public bool TryRevert()
		{
			var success = false;

			try
			{
				Initialize(true);
				success = Revert();
			}
			catch (Exception e)
			{
				logger.Error("Failed to revert operations!", e);
			}

			return success;
		}

		private void Initialize(bool indeterminate = false)
		{
			if (indeterminate)
			{
				ProgressIndicator?.SetIndeterminate();
			}
			else
			{
				ProgressIndicator?.SetValue(0);
				ProgressIndicator?.SetMaxValue(operations.Count);
			}
		}

		private OperationResult Perform()
		{
			foreach (var operation in operations)
			{
				var result = OperationResult.Failed;

				stack.Push(operation);

				try
				{
					operation.ProgressIndicator = ProgressIndicator;
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

				ProgressIndicator?.Progress();
			}

			return OperationResult.Success;
		}

		private OperationResult Repeat()
		{
			foreach (var operation in operations)
			{
				var result = OperationResult.Failed;

				try
				{
					operation.ProgressIndicator = ProgressIndicator;
					result = operation.Repeat();
				}
				catch (Exception e)
				{
					logger.Error($"Caught unexpected exception while repeating operation '{operation.GetType().Name}'!", e);
				}

				if (result != OperationResult.Success)
				{
					return result;
				}

				ProgressIndicator?.Progress();
			}

			return OperationResult.Success;
		}

		private bool Revert(bool regress = false)
		{
			var success = true;

			while (stack.Any())
			{
				var operation = stack.Pop();

				try
				{
					operation.ProgressIndicator = ProgressIndicator;
					operation.Revert();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to revert operation '{operation.GetType().Name}'!", e);
					success = false;
				}

				if (regress)
				{
					ProgressIndicator?.Regress();
				}
			}

			return success;
		}
	}
}
