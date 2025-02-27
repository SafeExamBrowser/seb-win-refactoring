/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class NetworkResponsibilityTests
	{
		private Mock<INetworkAdapter> networkAdapter;
		private Mock<IText> text;
		private Mock<IUserInterfaceFactory> uiFactory;

		[TestInitialize]
		public void Initialize()
		{
			var context = new ClientContext();
			var logger = new Mock<ILogger>();

			networkAdapter = new Mock<INetworkAdapter>();
			text = new Mock<IText>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			var sut = new NetworkResponsibility(context, logger.Object, networkAdapter.Object, text.Object, uiFactory.Object);
		}
	}
}
