/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.Network.Events;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class NetworkResponsibility : ClientResponsibility
	{
		private readonly INetworkAdapter networkAdapter;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		public NetworkResponsibility(ClientContext context, ILogger logger, INetworkAdapter networkAdapter, IText text, IUserInterfaceFactory uiFactory) : base(context, logger)
		{
			this.networkAdapter = networkAdapter;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.DeregisterEvents:
					DeregisterEvents();
					break;
				case ClientTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void DeregisterEvents()
		{
			networkAdapter.CredentialsRequired -= NetworkAdapter_CredentialsRequired;
		}

		private void RegisterEvents()
		{
			networkAdapter.CredentialsRequired += NetworkAdapter_CredentialsRequired;
		}

		private void NetworkAdapter_CredentialsRequired(CredentialsRequiredEventArgs args)
		{
			var message = text.Get(TextKey.CredentialsDialog_WirelessNetworkMessage).Replace("%%_NAME_%%", args.NetworkName);
			var title = text.Get(TextKey.CredentialsDialog_WirelessNetworkTitle);
			var dialog = uiFactory.CreateCredentialsDialog(CredentialsDialogPurpose.WirelessNetwork, message, title);
			var result = dialog.Show();

			args.Password = result.Password;
			args.Success = result.Success;
			args.Username = result.Username;
		}
	}
}
