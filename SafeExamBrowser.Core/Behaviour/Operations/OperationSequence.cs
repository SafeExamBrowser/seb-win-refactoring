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
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class OperationSequence : IOperationSequence
	{
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private ISplashScreen splashScreen;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		private Stack<IOperation> stack = new Stack<IOperation>();

		public OperationSequence(ILogger logger, IRuntimeInfo runtimeInfo, IText text, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.runtimeInfo = runtimeInfo;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryPerform(Queue<IOperation> operations)
		{
			var success = false;

			try
			{
				Initialize(operations.Count);
				success = Perform(operations);

				if (!success)
				{
					RevertOperations();
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to perform operations!", e);
			}
			finally
			{
				Finish();
			}

			return success;
		}

		public bool TryRepeat()
		{
			throw new NotImplementedException();
		}

		public bool TryRevert()
		{
			var success = false;

			try
			{
				Initialize();
				success = RevertOperations(false);
			}
			catch (Exception e)
			{
				logger.Error($"Failed to revert operations!", e);
			}
			finally
			{
				Finish();
			}

			return success;
		}

		private void Initialize(int? operationCount = null)
		{
			splashScreen = uiFactory.CreateSplashScreen(runtimeInfo, text);

			if (operationCount.HasValue)
			{
				splashScreen.SetMaxProgress(operationCount.Value);
			}
			else
			{
				splashScreen.SetIndeterminate();
			}

			splashScreen.UpdateText(TextKey.SplashScreen_StartupProcedure);
			splashScreen.Show();
		}

		private bool Perform(Queue<IOperation> operations)
		{
			foreach (var operation in operations)
			{
				stack.Push(operation);
				operation.SplashScreen = splashScreen;

				try
				{
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

				splashScreen.Progress();
			}

			return true;
		}

		private bool RevertOperations(bool regress = true)
		{
			var success = true;

			while (stack.Any())
			{
				var operation = stack.Pop();

				try
				{
					operation.Revert();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to revert operation '{operation.GetType().Name}'!", e);
					success = false;
				}

				if (regress)
				{
					splashScreen.Regress();
				}
			}

			return success;
		}

		private void Finish()
		{
			splashScreen?.Close();
		}
	}
}
