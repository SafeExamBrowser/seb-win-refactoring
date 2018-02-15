/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication
{
	public class ServiceProxy : BaseProxy, IServiceProxy
	{
		public bool Ignore { private get; set; }

		public ServiceProxy(string address, ILogger logger) : base(address, logger)
		{
		}

		public override bool Connect(Guid? token = null)
		{
			if (IgnoreOperation(nameof(Connect)))
			{
				return false;
			}

			return base.Connect();
		}

		public override bool Disconnect()
		{
			if (IgnoreOperation(nameof(Disconnect)))
			{
				return true;
			}

			return base.Disconnect();
		}

		public void StartSession(Guid sessionId, Settings settings)
		{
			if (IgnoreOperation(nameof(StartSession)))
			{
				return;
			}

			// TODO: Send(new StartSessionMessage { Id = sessionId, Settings = settings });
		}

		public void StopSession(Guid sessionId)
		{
			if (IgnoreOperation(nameof(StopSession)))
			{
				return;
			}

			// TODO
		}

		private bool IgnoreOperation(string operationName)
		{
			if (Ignore)
			{
				Logger.Debug($"Skipping {operationName} because the ignore flag is set.");
			}

			return Ignore;
		}
	}
}
