/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class SessionInitializationOperation : SessionOperation
	{
		private readonly IConfigurationRepository repository;
		private readonly IFileSystem fileSystem;

		public override event StatusChangedEventHandler StatusChanged;

		public SessionInitializationOperation(
			Dependencies dependencies,
			IFileSystem fileSystem,
			IConfigurationRepository repository) : base(dependencies)
		{
			this.fileSystem = fileSystem;
			this.repository = repository;
		}

		public override OperationResult Perform()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			FinalizeSessionConfiguration();

			return OperationResult.Success;
		}

		private void InitializeSessionConfiguration()
		{
			Logger.Info("Initializing new session configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeSession);

			Context.Next = repository.InitializeSessionConfiguration();

			Logger.Info($" -> Client-ID: {Context.Next.AppConfig.ClientId}");
			Logger.Info($" -> Runtime-ID: {Context.Next.AppConfig.RuntimeId}");
			Logger.Info($" -> Session-ID: {Context.Next.SessionId}");

			fileSystem.CreateDirectory(Context.Next.AppConfig.TemporaryDirectory);
		}

		private void FinalizeSessionConfiguration()
		{
			Context.Current = null;
			Context.Next = null;
		}
	}
}
