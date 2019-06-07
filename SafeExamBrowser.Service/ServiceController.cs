/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Service;

namespace SafeExamBrowser.Service
{
	internal class ServiceController : IServiceController
	{
		private IOperationSequence bootstrapSequence;
		private IRepeatableOperationSequence sessionSequence;
		private IServiceHost serviceHost;
		private SessionContext sessionContext;

		private object Session
		{
			get { return sessionContext.Current; }
		}

		private bool SessionIsRunning
		{
			get { return Session != null; }
		}

		public ServiceController(
			IOperationSequence bootstrapSequence,
			IRepeatableOperationSequence sessionSequence,
			IServiceHost serviceHost,
			SessionContext sessionContext)
		{
			this.bootstrapSequence = bootstrapSequence;
			this.sessionSequence = sessionSequence;
			this.serviceHost = serviceHost;
			this.sessionContext = sessionContext;
		}

		public bool TryStart()
		{
			var result = bootstrapSequence.TryPerform();
			var success = result == OperationResult.Success;

			return success;
		}

		public void Terminate()
		{
			var result = default(OperationResult);
			
			if (SessionIsRunning)
			{
				result = sessionSequence.TryRevert();
			}

			result = bootstrapSequence.TryRevert();
		}
	}
}
