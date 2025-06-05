/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class DisplayMonitorOperation : SessionOperation
	{
		private readonly IDisplayMonitor displayMonitor;

		public override event StatusChangedEventHandler StatusChanged;

		public DisplayMonitorOperation(Dependencies dependencies, IDisplayMonitor displayMonitor) : base(dependencies)
		{
			this.displayMonitor = displayMonitor;
		}

		public override OperationResult Perform()
		{
			return CheckDisplayConfiguration();
		}

		public override OperationResult Repeat()
		{
			return CheckDisplayConfiguration();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult CheckDisplayConfiguration()
		{
			Logger.Info("Validating display configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateDisplayConfiguration);

			var validation = displayMonitor.ValidateConfiguration(Context.Next.Settings.Display);
			var result = validation.IsAllowed ? OperationResult.Success : OperationResult.Failed;

			if (validation.IsAllowed)
			{
				Logger.Info("Display configuration is allowed.");
			}
			else
			{
				Logger.Error("Display configuration is not allowed!");
				ShowError(validation);
			}

			return result;
		}

		private void ShowError(ValidationResult validation)
		{
			var internalOnly = Text.Get(TextKey.MessageBox_DisplayConfigurationInternal);
			var internalOrExternal = Text.Get(TextKey.MessageBox_DisplayConfigurationInternalOrExternal);
			var message = TextKey.MessageBox_DisplayConfigurationError;
			var title = TextKey.MessageBox_DisplayConfigurationErrorTitle;
			var placeholders = new Dictionary<string, string>
			{
				{ "%%_ALLOWED_COUNT_%%", Convert.ToString(Context.Next.Settings.Display.AllowedDisplays) },
				{ "%%_EXTERNAL_COUNT_%%", Convert.ToString(validation.ExternalDisplays) },
				{ "%%_INTERNAL_COUNT_%%", Convert.ToString(validation.InternalDisplays) },
				{ "%%_TYPE_%%", Context.Next.Settings.Display.InternalDisplayOnly ? internalOnly : internalOrExternal }
			};

			ShowMessageBox(message, title, icon: MessageBoxIcon.Error, messagePlaceholders: placeholders);
		}
	}
}
