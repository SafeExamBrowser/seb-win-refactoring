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
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
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

		public bool TryPerform()
		{
			var success = false;

			try
			{
				Initialize();
				success = Perform();

				if (!success)
				{
					Revert();
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to perform operations!", e);
			}

			return success;
		}

		public bool TryRepeat()
		{
			var success = false;

			try
			{
				Initialize();
				success = Repeat();
			}
			catch (Exception e)
			{
				logger.Error("Failed to repeat operations!", e);
			}

			return success;
		}

		public bool TryRevert()
		{
			var success = false;

			try
			{
				Initialize();
				success = Revert(false);
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

		private bool Perform()
		{
			foreach (var operation in operations)
			{
				stack.Push(operation);

				try
				{
					operation.ProgressIndicator = ProgressIndicator;
					operation.Perform();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to perform operation '{operation.GetType().Name}'!", e);

					return false;
				}

				if (operation.Abort)
				{
					return false;
				}

				ProgressIndicator?.Progress();
			}

			return true;
		}

		private bool Repeat()
		{
			foreach (var operation in operations)
			{
				try
				{
					operation.ProgressIndicator = ProgressIndicator;
					operation.Repeat();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to repeat operation '{operation.GetType().Name}'!", e);

					return false;
				}

				if (operation.Abort)
				{
					return false;
				}

				ProgressIndicator?.Progress();
			}

			return true;
		}

		private bool Revert(bool regress = true)
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
