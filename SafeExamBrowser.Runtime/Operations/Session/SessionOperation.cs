/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	/// <summary>
	/// The base implementation to be used for all operations in the session operation sequence.
	/// </summary>
	internal abstract class SessionOperation : IRepeatableOperation
	{
		private readonly IMessageBox messageBox;

		protected ClientBridge ClientBridge { get; }
		protected RuntimeContext Context { get; }
		protected ILogger Logger { get; }
		protected IRuntimeWindow RuntimeWindow { get; }
		protected IText Text { get; }

		public abstract event StatusChangedEventHandler StatusChanged;

		internal SessionOperation(Dependencies dependencies)
		{
			ClientBridge = dependencies.ClientBridge;
			Context = dependencies.RuntimeContext;
			Logger = dependencies.Logger;
			messageBox = dependencies.MessageBox;
			RuntimeWindow = dependencies.RuntimeWindow;
			Text = dependencies.Text;
		}

		public abstract OperationResult Perform();
		public abstract OperationResult Repeat();
		public abstract OperationResult Revert();

		/// <summary>
		/// Shows a message box either directly or (if required) via the currently running client application component. All session operations
		/// should always use this method instead of using <see cref="IMessageBox"/> directly!
		/// </summary>
		protected MessageBoxResult ShowMessageBox(
			TextKey messageKey,
			TextKey titleKey,
			MessageBoxAction action = MessageBoxAction.Ok,
			MessageBoxIcon icon = MessageBoxIcon.Information,
			IDictionary<string, string> messagePlaceholders = default,
			IDictionary<string, string> titlePlaceholders = default)
		{
			var message = Text.Get(messageKey);
			var result = default(MessageBoxResult);
			var title = Text.Get(titleKey);

			foreach (var placeholder in messagePlaceholders ?? Enumerable.Empty<KeyValuePair<string, string>>())
			{
				message = message.Replace(placeholder.Key, placeholder.Value);
			}

			foreach (var placeholder in titlePlaceholders ?? Enumerable.Empty<KeyValuePair<string, string>>())
			{
				title = title.Replace(placeholder.Key, placeholder.Value);
			}

			if (ClientBridge.IsRequired())
			{
				result = ClientBridge.ShowMessageBox(message, title, action, icon);
			}
			else
			{
				result = messageBox.Show(message, title, action, icon, RuntimeWindow);
			}

			return result;
		}
	}
}
