/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading;
using CefSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;

namespace SafeExamBrowser.Browser.UnitTests.Handlers
{
	[TestClass]
	public class DialogHandlerTests
	{
		private DialogHandler sut;

		[TestInitialize]
		public void Initialize()
		{
			sut = new DialogHandler();
		}

		[TestMethod]
		public void MustCorrectlyCancelDialog()
		{
			RequestDialog(default, false);
		}

		[TestMethod]
		public void MustCorrectlyRequestOpenFileDialog()
		{
			var args = RequestDialog(CefFileDialogMode.Open);

			Assert.AreEqual(FileSystemElement.File, args.Element);
			Assert.AreEqual(FileSystemOperation.Open, args.Operation);
		}

		[TestMethod]
		public void MustCorrectlyRequestOpenFolderDialog()
		{
			var args = RequestDialog(CefFileDialogMode.OpenFolder);

			Assert.AreEqual(FileSystemElement.Folder, args.Element);
			Assert.AreEqual(FileSystemOperation.Open, args.Operation);
		}

		[TestMethod]
		public void MustCorrectlyRequestSaveFileDialog()
		{
			var args = RequestDialog(CefFileDialogMode.Save);

			Assert.AreEqual(FileSystemElement.File, args.Element);
			Assert.AreEqual(FileSystemOperation.Save, args.Operation);
		}

		private DialogRequestedEventArgs RequestDialog(CefFileDialogMode mode, bool confirm = true)
		{
			var args = default(DialogRequestedEventArgs);
			var callback = new Mock<IFileDialogCallback>();
			var title = "Some random dialog title";
			var initialPath = @"C:\Some\Random\Path";
			var sync = new AutoResetEvent(false);
			var threadId = default(int);

			callback.Setup(c => c.Cancel()).Callback(() => sync.Set());
			callback.Setup(c => c.Continue(It.IsAny<List<string>>())).Callback(() => sync.Set());
			sut.DialogRequested += (a) =>
			{
				args = a;
				args.Success = confirm;
				args.FullPath = @"D:\Some\Other\File\Path.txt";
				threadId = Thread.CurrentThread.ManagedThreadId;
			};

			var status = sut.OnFileDialog(default, default, mode, title, initialPath, default, callback.Object);

			sync.WaitOne();

			if (confirm)
			{
				callback.Verify(c => c.Continue(It.IsAny<List<string>>()), Times.Once);
				callback.Verify(c => c.Cancel(), Times.Never);
			}
			else
			{
				callback.Verify(c => c.Continue(It.IsAny<List<string>>()), Times.Never);
				callback.Verify(c => c.Cancel(), Times.Once);
			}

			Assert.IsTrue(status);
			Assert.AreEqual(initialPath, args.InitialPath);
			Assert.AreEqual(title, args.Title);
			Assert.AreNotEqual(threadId, Thread.CurrentThread.ManagedThreadId);

			return args;
		}
	}
}
