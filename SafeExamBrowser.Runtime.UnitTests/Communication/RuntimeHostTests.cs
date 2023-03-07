/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.UnitTests.Communication
{
	[TestClass]
	public class RuntimeHostTests
	{
		private Mock<IHostObject> hostObject;
		private Mock<IHostObjectFactory> hostObjectFactory;
		private Mock<ILogger> logger;
		private RuntimeHost sut;

		[TestInitialize]
		public void Initialize()
		{
			hostObject = new Mock<IHostObject>();
			hostObjectFactory = new Mock<IHostObjectFactory>();
			logger = new Mock<ILogger>();

			hostObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>(), It.IsAny<ICommunication>())).Returns(hostObject.Object);

			sut = new RuntimeHost("net:pipe://some/address", hostObjectFactory.Object, logger.Object, 0);
		}

		[TestMethod]
		public void MustAllowConnectionIfTokenIsValid()
		{
			var token = Guid.NewGuid();

			sut.AllowConnection = true;
			sut.AuthenticationToken = token;

			var response = sut.Connect(token);

			Assert.IsNotNull(response);
			Assert.IsTrue(response.ConnectionEstablished);
		}

		[TestMethod]
		public void MustRejectConnectionIfTokenInvalid()
		{
			var token = Guid.NewGuid();

			sut.AllowConnection = true;
			sut.AuthenticationToken = token;

			var response = sut.Connect(Guid.NewGuid());

			Assert.IsNotNull(response);
			Assert.IsFalse(response.ConnectionEstablished);
		}

		[TestMethod]
		public void MustRejectConnectionIfNoAuthenticationTokenSet()
		{
			var token = Guid.Empty;

			sut.AllowConnection = true;

			var response = sut.Connect(token);

			Assert.IsNotNull(response);
			Assert.IsFalse(response.ConnectionEstablished);
		}

		[TestMethod]
		public void MustOnlyAllowOneConcurrentConnection()
		{
			var token = Guid.NewGuid();

			sut.AllowConnection = true;
			sut.AuthenticationToken = token;

			var response1 = sut.Connect(token);
			var response2 = sut.Connect(token);
			var response3 = sut.Connect(token);

			Assert.IsNotNull(response1);
			Assert.IsNotNull(response2);
			Assert.IsNotNull(response3);
			Assert.IsNotNull(response1.CommunicationToken);
			Assert.IsNull(response2.CommunicationToken);
			Assert.IsNull(response3.CommunicationToken);
			Assert.IsTrue(response1.ConnectionEstablished);
			Assert.IsFalse(response2.ConnectionEstablished);
			Assert.IsFalse(response3.ConnectionEstablished);
		}

		[TestMethod]
		public void MustCorrectlyDisconnectClient()
		{
			var disconnected = false;
			var token = Guid.NewGuid();

			sut.AllowConnection = true;
			sut.AuthenticationToken = token;
			sut.ClientDisconnected += () => disconnected = true;

			var connectionResponse = sut.Connect(token);
			var message = new DisconnectionMessage
			{
				CommunicationToken = connectionResponse.CommunicationToken.Value,
				Interlocutor = Interlocutor.Client
			};
			var response = sut.Disconnect(message);

			Assert.IsNotNull(response);
			Assert.IsTrue(disconnected);
			Assert.IsTrue(response.ConnectionTerminated);
		}

		[TestMethod]
		public void MustAllowReconnectionAfterDisconnection()
		{
			var token = Guid.NewGuid();

			sut.AllowConnection = true;
			sut.AuthenticationToken = token;

			var response = sut.Connect(token);

			sut.Disconnect(new DisconnectionMessage { CommunicationToken = response.CommunicationToken.Value });
			sut.AllowConnection = true;
			sut.AuthenticationToken = token = Guid.NewGuid();

			response = sut.Connect(token);

			Assert.IsTrue(response.ConnectionEstablished);
		}

		[TestMethod]
		public void MustHandleClientReadyCorrectly()
		{
			var clientReady = false;

			sut.AllowConnection = true;
			sut.ClientReady += () => clientReady = true;
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.ClientIsReady) { CommunicationToken = token };
			var response = sut.Send(message);

			Assert.IsTrue(clientReady);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleConfigurationRequestCorrectly()
		{
			var args = default(ClientConfigurationEventArgs);
			var configuration = new ClientConfiguration { Settings = new AppSettings() };

			configuration.Settings.Security.AdminPasswordHash = "12345";
			sut.AllowConnection = true;
			sut.ClientConfigurationNeeded += (a) => { args = a; args.ClientConfiguration = configuration; };
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.ConfigurationNeeded) { CommunicationToken = token };
			var response = sut.Send(message);

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(ConfigurationResponse));
			Assert.AreEqual(configuration.Settings.Security.AdminPasswordHash, (response as ConfigurationResponse)?.Configuration.Settings.Security.AdminPasswordHash);
		}

		[TestMethod]
		public void MustHandleExamSelectionCorrectly()
		{
			var args = default(ExamSelectionReplyEventArgs);
			var examId = "abc123";
			var requestId = Guid.NewGuid();
			var sync = new AutoResetEvent(false);

			sut.AllowConnection = true;
			sut.AuthenticationToken = Guid.Empty;
			sut.ExamSelectionReceived += (a) => { args = a; sync.Set(); };

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new ExamSelectionReplyMessage(requestId, true, examId) { CommunicationToken = token };
			var response = sut.Send(message);

			sync.WaitOne();

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
			Assert.IsTrue(args.Success);
			Assert.AreEqual(examId, args.SelectedExamId);
			Assert.AreEqual(requestId, args.RequestId);
		}

		[TestMethod]
		public void MustHandleServerFailureActionCorrectly()
		{
			var args = default(ServerFailureActionReplyEventArgs);
			var requestId = Guid.NewGuid();
			var sync = new AutoResetEvent(false);

			sut.AllowConnection = true;
			sut.AuthenticationToken = Guid.Empty;
			sut.ServerFailureActionReceived += (a) => { args = a; sync.Set(); };

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new ServerFailureActionReplyMessage(true, false, true, requestId) { CommunicationToken = token };
			var response = sut.Send(message);

			sync.WaitOne();

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
			Assert.IsFalse(args.Fallback);
			Assert.IsTrue(args.Abort);
			Assert.IsTrue(args.Retry);
			Assert.AreEqual(requestId, args.RequestId);
		}

		[TestMethod]
		public void MustHandleShutdownRequestCorrectly()
		{
			var shutdownRequested = false;

			sut.AllowConnection = true;
			sut.ShutdownRequested += () => shutdownRequested = true;
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.RequestShutdown) { CommunicationToken = token };
			var response = sut.Send(message);

			Assert.IsTrue(shutdownRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleMessageBoxReplyCorrectly()
		{
			var args = default(MessageBoxReplyEventArgs);
			var requestId = Guid.NewGuid();
			var result = (int) MessageBoxResult.Ok;
			var sync = new AutoResetEvent(false);

			sut.AllowConnection = true;
			sut.MessageBoxReplyReceived += (a) => { args = a; sync.Set(); };
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new MessageBoxReplyMessage(requestId, result) { CommunicationToken = token };
			var response = sut.Send(message);

			sync.WaitOne();

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
			Assert.AreEqual(requestId, args.RequestId);
			Assert.AreEqual(result, args.Result);
		}

		[TestMethod]
		public void MustHandlePasswordReplyCorrectly()
		{
			var args = default(PasswordReplyEventArgs);
			var password = "test1234";
			var requestId = Guid.NewGuid();
			var success = true;
			var sync = new AutoResetEvent(false);

			sut.AllowConnection = true;
			sut.PasswordReceived += (a) => { args = a; sync.Set(); };
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new PasswordReplyMessage(requestId, success, password) { CommunicationToken = token };
			var response = sut.Send(message);

			sync.WaitOne();

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
			Assert.AreEqual(password, args.Password);
			Assert.AreEqual(requestId, args.RequestId);
			Assert.AreEqual(success, args.Success);
		}

		[TestMethod]
		public void MustHandleReconfigurationRequestCorrectly()
		{
			var args = default(ReconfigurationEventArgs);
			var path = "C:\\Temp\\Some\\File.seb";
			var url = @"https://www.host.abc/someresource.seb";
			var sync = new AutoResetEvent(false);

			sut.AllowConnection = true;
			sut.ReconfigurationRequested += (a) => { args = a; sync.Set(); };
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new ReconfigurationMessage(path, url) { CommunicationToken = token };
			var response = sut.Send(message);

			sync.WaitOne();

			Assert.IsNotNull(args);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
			Assert.AreEqual(path, args.ConfigurationPath);
			Assert.AreEqual(url, args.ResourceUrl);
		}

		[TestMethod]
		public void MustReturnUnknownMessageAsDefault()
		{
			sut.AllowConnection = true;
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new TestMessage { CommunicationToken = token } as Message;
			var response = sut.Send(message);

			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.UnknownMessage, (response as SimpleResponse)?.Purport);

			message = new SimpleMessage(default(SimpleMessagePurport)) { CommunicationToken = token };
			response = sut.Send(message);

			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.UnknownMessage, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustNotFailIfNoEventHandlersSubscribed()
		{
			sut.AllowConnection = true;
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;

			sut.Send(new SimpleMessage(SimpleMessagePurport.ClientIsReady) { CommunicationToken = token });
			sut.Send(new SimpleMessage(SimpleMessagePurport.ConfigurationNeeded) { CommunicationToken = token });
			sut.Send(new SimpleMessage(SimpleMessagePurport.RequestShutdown) { CommunicationToken = token });
			sut.Send(new MessageBoxReplyMessage(Guid.Empty, (int) MessageBoxResult.Cancel) { CommunicationToken = token });
			sut.Send(new PasswordReplyMessage(Guid.Empty, false, "") { CommunicationToken = token });
			sut.Send(new ReconfigurationMessage("", "") { CommunicationToken = token });
			sut.Disconnect(new DisconnectionMessage { CommunicationToken = token });
		}

		private class TestMessage : Message { };
	}
}
