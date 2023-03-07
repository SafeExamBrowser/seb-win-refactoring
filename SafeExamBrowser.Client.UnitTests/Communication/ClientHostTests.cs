/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Client.UnitTests.Communication
{
	[TestClass]
	public class ClientHostTests
	{
		private const int PROCESS_ID = 1234;

		private Mock<IConfigurationRepository> configuration;
		private Mock<IHostObject> hostObject;
		private Mock<IHostObjectFactory> hostObjectFactory;
		private Mock<ILogger> logger;
		private ClientHost sut;

		[TestInitialize]
		public void Initialize()
		{
			configuration = new Mock<IConfigurationRepository>();
			hostObject = new Mock<IHostObject>();
			hostObjectFactory = new Mock<IHostObjectFactory>();
			logger = new Mock<ILogger>();

			hostObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>(), It.IsAny<ICommunication>())).Returns(hostObject.Object);

			sut = new ClientHost("net:pipe://some/address", hostObjectFactory.Object, logger.Object, PROCESS_ID, 0);
		}

		[TestMethod]
		public void MustOnlyAllowConnectionIfTokenIsValid()
		{
			var token = Guid.NewGuid();

			sut.AuthenticationToken = token;

			var response = sut.Connect(token);

			Assert.IsNotNull(response);
			Assert.IsTrue(response.ConnectionEstablished);
			Assert.IsTrue(sut.IsConnected);
		}


		[TestMethod]
		public void MustRejectConnectionIfTokenInvalid()
		{
			var token = Guid.NewGuid();

			sut.AuthenticationToken = token;

			var response = sut.Connect(Guid.NewGuid());

			Assert.IsNotNull(response);
			Assert.IsFalse(response.ConnectionEstablished);
			Assert.IsFalse(sut.IsConnected);
		}

		[TestMethod]
		public void MustOnlyAllowOneConcurrentConnection()
		{
			var token = Guid.NewGuid();

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
		public void MustCorrectlyDisconnect()
		{
			var eventFired = false;
			var token = Guid.NewGuid();

			sut.RuntimeDisconnected += () => eventFired = true;
			sut.AuthenticationToken = token;

			var connectionResponse = sut.Connect(token);
			var message = new DisconnectionMessage
			{
				CommunicationToken = connectionResponse.CommunicationToken.Value,
				Interlocutor = Interlocutor.Runtime
			};
			var response = sut.Disconnect(message);

			Assert.IsNotNull(response);
			Assert.IsTrue(response.ConnectionTerminated);
			Assert.IsTrue(eventFired);
			Assert.IsFalse(sut.IsConnected);
		}

		[TestMethod]
		public void MustNotAllowReconnectionAfterDisconnection()
		{
			var token = sut.AuthenticationToken = Guid.NewGuid();
			var response = sut.Connect(token);
			var message = new DisconnectionMessage
			{
				CommunicationToken = response.CommunicationToken.Value,
				Interlocutor = Interlocutor.Runtime
			};

			sut.Disconnect(message);
			sut.AuthenticationToken = token = Guid.NewGuid();

			response = sut.Connect(token);

			Assert.IsFalse(response.ConnectionEstablished);
		}

		[TestMethod]
		public void MustHandleAuthenticationRequestCorrectly()
		{
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.Authenticate) { CommunicationToken = token };
			var response = sut.Send(message);

			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(AuthenticationResponse));
			Assert.AreEqual(PROCESS_ID, (response as AuthenticationResponse)?.ProcessId);
		}

		[TestMethod]
		public void MustHandleExamSelectionRequestCorrectly()
		{
			var examSelectionRequested = false;
			var requestId = Guid.NewGuid();
			var resetEvent = new AutoResetEvent(false);

			sut.ExamSelectionRequested += (args) =>
			{
				examSelectionRequested = true;
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var request = new ExamSelectionRequestMessage(Enumerable.Empty<(string id, string lms, string name, string url)>(), requestId) { CommunicationToken = token };
			var response = sut.Send(request);

			resetEvent.WaitOne();

			Assert.IsTrue(examSelectionRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleMessageBoxRequestCorrectly()
		{
			var action = (int) MessageBoxAction.YesNo;
			var icon = (int) MessageBoxIcon.Question;
			var message = "Qwert kndorz safie abcd?";
			var messageBoxRequested = false;
			var requestId = Guid.NewGuid();
			var resetEvent = new AutoResetEvent(false);
			var title = "Poiuztrewq!";

			sut.MessageBoxRequested += (args) =>
			{
				messageBoxRequested = args.Action == action && args.Icon == icon && args.Message == message && args.RequestId == requestId && args.Title == title;
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var request = new MessageBoxRequestMessage(action, icon, message, requestId, title) { CommunicationToken = token };
			var response = sut.Send(request);

			resetEvent.WaitOne();

			Assert.IsTrue(messageBoxRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandlePasswordRequestCorrectly()
		{
			var passwordRequested = false;
			var purpose = PasswordRequestPurpose.LocalAdministrator;
			var requestId = Guid.NewGuid();
			var resetEvent = new AutoResetEvent(false);

			sut.PasswordRequested += (args) =>
			{
				passwordRequested = args.Purpose == purpose && args.RequestId == requestId;
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new PasswordRequestMessage(purpose, requestId) { CommunicationToken = token };
			var response = sut.Send(message);

			resetEvent.WaitOne();

			Assert.IsTrue(passwordRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleReconfigurationAbortionCorrectly()
		{
			var reconfigurationAborted = false;
			var resetEvent = new AutoResetEvent(false);

			sut.ReconfigurationAborted += () =>
			{
				reconfigurationAborted = true;
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.ReconfigurationAborted) { CommunicationToken = token };
			var response = sut.Send(message);

			resetEvent.WaitOne();

			Assert.IsTrue(reconfigurationAborted);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleReconfigurationDenialCorrectly()
		{
			var filePath = @"C:\Some\Random\Path\To\A\File.seb";
			var reconfigurationDenied = false;
			var resetEvent = new AutoResetEvent(false);

			sut.ReconfigurationDenied += (args) =>
			{
				reconfigurationDenied = new Uri(args.ConfigurationPath).Equals(new Uri(filePath));
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new ReconfigurationDeniedMessage(filePath) { CommunicationToken = token };
			var response = sut.Send(message);

			resetEvent.WaitOne();

			Assert.IsTrue(reconfigurationDenied);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleServerFailureCorrectly()
		{
			var serverFailureActionRequested = false;
			var requestId = Guid.NewGuid();
			var resetEvent = new AutoResetEvent(false);

			sut.ServerFailureActionRequested += (args) =>
			{
				serverFailureActionRequested = true;
				resetEvent.Set();
			};
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var request = new ServerFailureActionRequestMessage("", true, requestId) { CommunicationToken = token };
			var response = sut.Send(request);

			resetEvent.WaitOne();

			Assert.IsTrue(serverFailureActionRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustHandleShutdownRequestCorrectly()
		{
			var shutdownRequested = false;

			sut.Shutdown += () => shutdownRequested = true;
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;
			var message = new SimpleMessage(SimpleMessagePurport.Shutdown) { CommunicationToken = token };
			var response = sut.Send(message);

			Assert.IsTrue(shutdownRequested);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(SimpleResponse));
			Assert.AreEqual(SimpleResponsePurport.Acknowledged, (response as SimpleResponse)?.Purport);
		}

		[TestMethod]
		public void MustReturnUnknownMessageAsDefault()
		{
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
			sut.AuthenticationToken = Guid.Empty;

			var token = sut.Connect(Guid.Empty).CommunicationToken.Value;

			sut.Send(new MessageBoxRequestMessage(default(int), default(int), "", Guid.Empty, "") { CommunicationToken = token });
			sut.Send(new PasswordRequestMessage(default(PasswordRequestPurpose), Guid.Empty) { CommunicationToken = token });
			sut.Send(new SimpleMessage(SimpleMessagePurport.ReconfigurationAborted));
			sut.Send(new ReconfigurationDeniedMessage("") { CommunicationToken = token });
			sut.Send(new SimpleMessage(SimpleMessagePurport.Shutdown) { CommunicationToken = token });
			sut.Disconnect(new DisconnectionMessage { CommunicationToken = token, Interlocutor = Interlocutor.Runtime });
		}

		private class TestMessage : Message { };
	}
}
