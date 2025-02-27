/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications.UnitTests
{
	[TestClass]
	public class ExternalApplicationWindowTests
	{
		private IntPtr handle;
		private NativeIconResource icon;
		private Mock<INativeMethods> nativeMethods;

		private ExternalApplicationWindow sut;

		[TestInitialize]
		public void Initialize()
		{
			handle = new IntPtr(123);
			icon = new NativeIconResource();
			nativeMethods = new Mock<INativeMethods>();

			sut = new ExternalApplicationWindow(icon, nativeMethods.Object, handle);
		}

		[TestMethod]
		public void Activate_MustCorrectlyActivateWindow()
		{
			sut.Activate();
			nativeMethods.Verify(n => n.ActivateWindow(It.Is<IntPtr>(h => h == handle)));
		}

		[TestMethod]
		public void Update_MustCorrectlyUpdateWindow()
		{
			var iconChanged = false;
			var titleChanged = false;

			nativeMethods.Setup(m => m.GetWindowIcon(It.IsAny<IntPtr>())).Returns(new IntPtr(456));
			nativeMethods.Setup(m => m.GetWindowTitle((It.IsAny<IntPtr>()))).Returns("Some New Window Title");

			sut.IconChanged += (_) => iconChanged = true;
			sut.TitleChanged += (_) => titleChanged = true;

			sut.Update();

			nativeMethods.Verify(m => m.GetWindowIcon(handle), Times.Once);
			nativeMethods.Verify(m => m.GetWindowTitle(handle), Times.Once);

			Assert.IsTrue(iconChanged);
			Assert.IsTrue(titleChanged);
		}
	}
}
