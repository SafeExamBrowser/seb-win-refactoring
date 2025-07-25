﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Client.UnitTests.Responsibilities
{
	[TestClass]
	public class ProctoringResponsibilityTests
	{
		private ClientContext context;
		private Mock<ILogger> logger;
		private Mock<IMessageBox> messageBox;
		private Mock<IUserInterfaceFactory> uiFactory;

		private ProctoringResponsibility sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new ClientContext();
			logger = new Mock<ILogger>();
			messageBox = new Mock<IMessageBox>();
			uiFactory = new Mock<IUserInterfaceFactory>();

			sut = new ProctoringResponsibility(context, logger.Object, messageBox.Object, uiFactory.Object);
		}

		[TestMethod]
		public void MustNotFailIfDependencyIsNull()
		{
			context.Proctoring = null;
			sut.Assume(ClientTask.PrepareShutdown_Wave1);
		}
	}
}
