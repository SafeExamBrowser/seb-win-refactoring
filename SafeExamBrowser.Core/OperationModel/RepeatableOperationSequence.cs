/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.OperationModel
{
	/// <summary>
	/// Default implementation of the <see cref="IRepeatableOperationSequence"/>.
	/// </summary>
	public class RepeatableOperationSequence : OperationSequence, IRepeatableOperationSequence
	{
		private new Queue<IRepeatableOperation> operations;

		public RepeatableOperationSequence(ILogger logger, Queue<IRepeatableOperation> operations) : base(logger, new Queue<IOperation>(operations))
		{
			this.operations = new Queue<IRepeatableOperation>(operations);
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

		private OperationResult Repeat()
		{
			foreach (var operation in operations)
			{
				var result = OperationResult.Failed;

				try
				{
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

				UpdateProgress(new ProgressChangedEventArgs { Progress = true });
			}

			return OperationResult.Success;
		}
	}
}
