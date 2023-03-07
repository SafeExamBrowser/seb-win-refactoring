/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Logging.UnitTests
{
	[TestClass]
	public class LoggerTests
	{
		[TestMethod]
		public void MustAddMessagesToLog()
		{
			var sut = new Logger();
			var debug = "I'm a debug message";
			var info = "I'm an info message";
			var warn = "I'm a warning!";
			var error = "I AM AN ERROR!!";
			var exceptionMessage = "I'm an exception message";
			var exception = new Exception(exceptionMessage);
			var message = "I'm a simple text message";

			sut.Debug(debug);
			sut.Info(info);
			sut.Warn(warn);
			sut.Error(error);
			sut.Error(error, exception);
			sut.Log(message);

			var log = sut.GetLog();

			Assert.IsTrue(log.Count == 7);

			Assert.IsTrue(debug.Equals((log[0] as ILogMessage).Message));
			Assert.IsTrue((log[0] as ILogMessage).Severity == LogLevel.Debug);

			Assert.IsTrue(info.Equals((log[1] as ILogMessage).Message));
			Assert.IsTrue((log[1] as ILogMessage).Severity == LogLevel.Info);

			Assert.IsTrue(warn.Equals((log[2] as ILogMessage).Message));
			Assert.IsTrue((log[2] as ILogMessage).Severity == LogLevel.Warning);

			Assert.IsTrue(error.Equals((log[3] as ILogMessage).Message));
			Assert.IsTrue((log[3] as ILogMessage).Severity == LogLevel.Error);

			Assert.IsTrue(error.Equals((log[4] as ILogMessage).Message));
			Assert.IsTrue((log[4] as ILogMessage).Severity == LogLevel.Error);
			Assert.IsTrue((log[5] as ILogText).Text.Contains(exceptionMessage));

			Assert.IsTrue(message.Equals((log[6] as ILogText).Text));
		}

		[TestMethod]
		public void MustAddInnerExceptionsToLog()
		{
			var sut = new Logger();
			var outerMessage = "Some message for the outer exception";
			var innerMessage = "BAAAAM! Inner one here.";
			var innerInnerMessage = "Yikes, a null reference...";
			var exception = new Exception(outerMessage, new ArgumentException(innerMessage, new NullReferenceException(innerInnerMessage)));

			sut.Error("blubb", exception);

			var log = sut.GetLog();
			var logText = log[1] as ILogText;

			Assert.AreEqual(2, log.Count);
			Assert.IsTrue(logText.Text.Contains(outerMessage));
			Assert.IsTrue(logText.Text.Contains(innerMessage));
			Assert.IsTrue(logText.Text.Contains(innerInnerMessage));
		}

		[TestMethod]
		public void MustReturnCopyOfLog()
		{
			var sut = new Logger();
			var debug = "I'm a debug message";
			var info = "I'm an info message";
			var warn = "I'm a warning!";
			var error = "I AM AN ERROR!!";

			sut.Debug(debug);
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

			Assert.ThrowsException<ArgumentNullException>(() => sut.Debug(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Info(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Warn(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null, null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error("Hello world!", null));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Error(null, new Exception()));
			Assert.ThrowsException<ArgumentNullException>(() => sut.Log((string) null));
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
		public void MustRespectLogLevel()
		{
			var sut = new Logger();

			sut.LogLevel = LogLevel.Error;
			sut.Debug("debug");
			sut.Info("info");
			sut.Warn("warn");

			Assert.AreEqual(0, sut.GetLog().Count);

			sut = new Logger();
			sut.LogLevel = LogLevel.Warning;
			sut.Debug("debug");
			sut.Info("info");
			sut.Warn("warn");

			Assert.AreEqual(1, sut.GetLog().Count);

			sut = new Logger();
			sut.LogLevel = LogLevel.Info;
			sut.Debug("debug");
			sut.Info("info");
			sut.Warn("warn");

			Assert.AreEqual(2, sut.GetLog().Count);

			sut = new Logger();
			sut.LogLevel = LogLevel.Debug;
			sut.Debug("debug");
			sut.Info("info");
			sut.Warn("warn");

			Assert.AreEqual(3, sut.GetLog().Count);
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
