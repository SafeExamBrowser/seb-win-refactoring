/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class Dependencies
	{
		public ClientBridge ClientBridge { get; }
		internal ILogger Logger { get; }
		internal IMessageBox MessageBox { get; }
		internal RuntimeContext RuntimeContext { get; }
		internal IRuntimeWindow RuntimeWindow { get; }
		internal IText Text { get; }

		internal Dependencies(
			ClientBridge clientBridge,
			ILogger logger,
			IMessageBox messageBox,
			IRuntimeWindow runtimeWindow,
			RuntimeContext runtimeContext,
			IText text)
		{
			ClientBridge = clientBridge ?? throw new ArgumentNullException(nameof(clientBridge));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			MessageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
			RuntimeWindow = runtimeWindow ?? throw new ArgumentNullException(nameof(runtimeWindow));
			RuntimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}
	}
}
