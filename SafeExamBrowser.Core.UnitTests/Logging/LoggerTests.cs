/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Logging;

namespace SafeExamBrowser.Core.UnitTests.Logging
{
	[TestClass]
	public class LoggerTests
	{
		[TestMethod]
		public void MustAddMessagesToLog()
		{
			var sut = new Logger();
			var info = "I'm an info message";
			var warn = "I'm a warning!";
			var error = "I AM AN ERROR!!";
			var exceptionMessage = "I'm an exception message";
			var exception = new Exception(exceptionMessage);
			var message = "I'm a simple text message";
			var content = new LogText("I'm some raw log text...");

			sut.Info(info);
			sut.Warn(warn);
			sut.Error(error);
			sut.Error(error, exception);
			sut.Log(message);
			sut.Log(content);

			var log = sut.GetLog();

			Assert.IsTrue(log.Count == 7);

			Assert.IsTrue(info.Equals((log[0] as ILogMessage).Message));
			Assert.IsTrue((log[0] as ILogMessage).Severity == LogLevel.Info);

			Assert.IsTrue(warn.Equals((log[1] as ILogMessage).Message));
			Assert.IsTrue((log[1] as ILogMessage).Severity == LogLevel.Warning);

			Assert.IsTrue(error.Equals((log[2] as ILogMessage).Message));
			Assert.IsTrue((log[2] as ILogMessage).Severity == LogLevel.Error);

			Assert.IsTrue(error.Equals((log[3] as ILogMessage).Message));
			Assert.IsTrue((log[3] as ILogMessage).Severity == LogLevel.Error);
			Assert.IsTrue((log[4] as ILogText).Text.Contains(exceptionMessage));

			Assert.IsTrue(message.Equals((log[5] as ILogText).Text));

			Assert.IsTrue(content.Text.Equals((log[6] as ILogText).Text));
		}

		[TestMethod]
		public void MustReturnCopyOfLog()
		{
			var sut = new Logger();
			var info = "I'm an info message";
			var warn = "I'm a warning!";
			var error = "I AM AN ERROR!!";

			sut.Info(info);
			sut.Warn(warn);
			sut.Error(error);

			var log1 = sut.GetLog();
			var log2 = sut.GetLog();

			Assert.AreNotSame(log1, log2);

			foreach (var message in log1)
			{
				Assert.AreNotSame(message, log2[log1.IndexOf(message)]);
			}
		}

		[TestMethod]
		public void MustNotAllowLoggingNull()
		{
			var sut = new Logger();

			Assert.ThrowsException<ArgumentNullException>(() => sut.Info(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Warn(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null, null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error("Hello world!", null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null, new Exception()));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Log((string) null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Log((ILogContent) null));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullObserver()
		{
			var sut = new Logger();

			sut.Subscribe(null);
		}

		[TestMethod]
		public void MustNotSubscribeSameObserverMultipleTimes()
		{
			var sut = new Logger();
			var observer = new Mock<ILogObserver>();

			observer.Setup(o => o.Notify(It.IsAny<ILogMessage>()));

			sut.Subscribe(observer.Object);
			sut.Subscribe(observer.Object);
			sut.Subscribe(observer.Object);

			sut.Info("Blubb");

			observer.Verify(o => o.Notify(It.IsAny<ILogMessage>()), Times.Once());
		}

		[TestMethod]
		public void MustNotFailWhenRemovingNullObserver()
		{
			var sut = new Logger();

			sut.Unsubscribe(null);
		}

		[TestMethod]
		public void MustSubscribeObserver()
		{
			var sut = new Logger();
			var observer = new Mock<ILogObserver>();
			var message = "Blubb";
			var messages = new List<ILogContent>();

			observer.Setup(o => o.Notify(It.IsAny<ILogContent>())).Callback<ILogContent>(m => messages.Add(m));

			sut.Subscribe(observer.Object);
			sut.Info(message);
			sut.Warn(message);

			observer.Verify(o => o.Notify(It.IsAny<ILogContent>()), Times.Exactly(2));

			Assert.IsTrue(messages.Count == 2);

			Assert.IsTrue((messages[0] as ILogMessage).Severity == LogLevel.Info);
			Assert.IsTrue(message.Equals((messages[0] as ILogMessage).Message));

			Assert.IsTrue((messages[1] as ILogMessage).Severity == LogLevel.Warning);
			Assert.IsTrue(message.Equals((messages[1] as ILogMessage).Message));
		}

		[TestMethod]
		public void MustUnsubscribeObserver()
		{
			var sut = new Logger();
			var observer = new Mock<ILogObserver>();
			var message = "Blubb";
			var messages = new List<ILogContent>();

			observer.Setup(o => o.Notify(It.IsAny<ILogContent>())).Callback<ILogContent>(m => messages.Add(m));

			sut.Subscribe(observer.Object);
			sut.Info(message);
			sut.Unsubscribe(observer.Object);
			sut.Warn(message);

			observer.Verify(o => o.Notify(It.IsAny<ILogContent>()), Times.Once());

			Assert.IsTrue(messages.Count == 1);

			Assert.IsTrue((messages[0] as ILogMessage).Severity == LogLevel.Info);
			Assert.IsTrue(message.Equals((messages[0] as ILogMessage).Message));
		}
	}
}
