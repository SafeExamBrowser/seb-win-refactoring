/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications.UnitTests
{
	[TestClass]
	public class ExternalApplicationInstanceTests
	{
		private NativeIconResource icon;
		private Mock<ILogger> logger;
		private Mock<INativeMethods> nativeMethods;
		private Mock<IProcess> process;
		private ExternalApplicationInstance sut;

		[TestInitialize]
		public void Initialize()
		{
			icon = new NativeIconResource();
			logger = new Mock<ILogger>();
			nativeMethods = new Mock<INativeMethods>();
			process = new Mock<IProcess>();

			sut = new ExternalApplicationInstance(icon, logger.Object, nativeMethods.Object, process.Object, 1);
		}

		[TestMethod]
		public void Terminate_MustDoNothingIfAlreadyTerminated()
		{
			process.SetupGet(p => p.HasTerminated).Returns(true);

			sut.Terminate();

			process.Verify(p => p.TryClose(It.IsAny<int>()), Times.Never());
			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.Never());
		}

		[TestMethod]
		public void Terminate_MustLogIfTerminationFailed()
		{
			process.Setup(p => p.TryClose(It.IsAny<int>())).Returns(false);
			process.Setup(p => p.TryKill(It.IsAny<int>())).Returns(false);
			process.SetupGet(p => p.HasTerminated).Returns(false);

			sut.Terminate();

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce);
			process.Verify(p => p.TryClose(It.IsAny<int>()), Times.AtLeastOnce());
			process.Verify(p => p.TryKill(It.IsAny<int>()), Times.AtLeastOnce());
		}
	}
}
