/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class DisclaimerOperationTests
	{
		private Mock<ILogger> logger;
		private AppSettings settings;
		private SessionContext context;

		private DisclaimerOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			context = new SessionContext();
			logger = new Mock<ILogger>();
			settings = new AppSettings();

			context.Next = new SessionConfiguration();
			context.Next.Settings = settings;
			sut = new DisclaimerOperation(logger.Object, context);
		}

		[TestMethod]
		public void Perform_MustShowDisclaimerWhenProctoringEnabled()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Ok;
				}
			};

			var result = sut.Perform();

			Assert.IsTrue(disclaimerShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustAbortIfDisclaimerNotConfirmed()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Cancel;
				}
			};

			var result = sut.Perform();

			Assert.IsTrue(disclaimerShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Perform_MustDoNothingIfProctoringNotEnabled()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = false;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Cancel;
				}
			};

			var result = sut.Perform();

			Assert.IsFalse(disclaimerShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustShowDisclaimerWhenProctoringEnabled()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Ok;
				}
			};

			var result = sut.Repeat();

			Assert.IsTrue(disclaimerShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Repeat_MustAbortIfDisclaimerNotConfirmed()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Cancel;
				}
			};

			var result = sut.Repeat();

			Assert.IsTrue(disclaimerShown);
			Assert.AreEqual(OperationResult.Aborted, result);
		}

		[TestMethod]
		public void Repeat_MustDoNothingIfProctoringNotEnabled()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = false;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Cancel;
				}
			};

			var result = sut.Repeat();

			Assert.IsFalse(disclaimerShown);
			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustDoNothing()
		{
			var disclaimerShown = false;

			settings.Proctoring.Enabled = true;
			sut.ActionRequired += (args) =>
			{
				if (args is MessageEventArgs m)
				{
					disclaimerShown = true;
					m.Result = MessageBoxResult.Cancel;
				}
			};

			var result = sut.Revert();

			Assert.IsFalse(disclaimerShown);
			Assert.AreEqual(OperationResult.Success, result);
		}
	}
}
